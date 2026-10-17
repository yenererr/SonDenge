"""Convert the bundled CC BY glTF models to Unity-importable OBJ assets.

Run from the repository root with Python and NumPy:
    python ThirdParty/Wilson/convert_glb.py
"""

import json
import struct
from pathlib import Path

import numpy as np


SOURCE = Path(__file__).resolve().parent
OUTPUT = SOURCE.parents[1] / "Assets" / "Resources" / "Wilson"
TYPES = {5120: np.int8, 5121: np.uint8, 5122: np.int16,
         5123: np.uint16, 5125: np.uint32, 5126: np.float32}
WIDTHS = {"SCALAR": 1, "VEC2": 2, "VEC3": 3, "VEC4": 4, "MAT4": 16}


def load_glb(path):
    data = path.read_bytes()
    if data[:4] != b"glTF" or struct.unpack_from("<I", data, 4)[0] != 2:
        raise ValueError(f"Not a glTF 2.0 binary: {path}")
    offset, chunks = 12, {}
    while offset < len(data):
        length, kind = struct.unpack_from("<I4s", data, offset)
        offset += 8
        chunks[kind] = data[offset:offset + length]
        offset += length
    return json.loads(chunks[b"JSON"]), chunks[b"BIN\x00"]


def accessor(document, binary, index):
    info = document["accessors"][index]
    view = document["bufferViews"][info["bufferView"]]
    dtype = np.dtype(TYPES[info["componentType"]]).newbyteorder("<")
    width = WIDTHS[info["type"]]
    stride = view.get("byteStride", dtype.itemsize * width)
    start = view.get("byteOffset", 0) + info.get("byteOffset", 0)
    values = np.ndarray((info["count"], width), dtype=dtype, buffer=binary,
                        offset=start, strides=(stride, dtype.itemsize)).copy()
    if info.get("normalized"):
        if np.issubdtype(dtype, np.signedinteger):
            values = np.maximum(values.astype(np.float64) / np.iinfo(dtype).max, -1)
        else:
            values = values.astype(np.float64) / np.iinfo(dtype).max
    return values


def node_matrix(node):
    if "matrix" in node:
        return np.asarray(node["matrix"], dtype=np.float64).reshape((4, 4), order="F")
    t = node.get("translation", [0, 0, 0])
    x, y, z, w = node.get("rotation", [0, 0, 0, 1])
    s = node.get("scale", [1, 1, 1])
    rotation = np.array([
        [1 - 2 * (y*y + z*z), 2 * (x*y - z*w), 2 * (x*z + y*w)],
        [2 * (x*y + z*w), 1 - 2 * (x*x + z*z), 2 * (y*z - x*w)],
        [2 * (x*z - y*w), 2 * (y*z + x*w), 1 - 2 * (x*x + y*y)],
    ])
    result = np.eye(4)
    result[:3, :3] = rotation @ np.diag(s)
    result[:3, 3] = t
    return result


def world_matrices(document):
    nodes = document["nodes"]
    parents = {}
    for index, node in enumerate(nodes):
        for child in node.get("children", []):
            parents[child] = index
    cache = {}

    def world(index):
        if index not in cache:
            matrix = node_matrix(nodes[index])
            cache[index] = world(parents[index]) @ matrix if index in parents else matrix
        return cache[index]

    return [world(index) for index in range(len(nodes))]


def export(name):
    document, binary = load_glb(SOURCE / f"{name}.glb")
    nodes = document["nodes"]
    node_index = next(i for i, node in enumerate(nodes) if "mesh" in node)
    node = nodes[node_index]
    primitive = document["meshes"][node["mesh"]]["primitives"][0]
    attrs = primitive["attributes"]
    positions = accessor(document, binary, attrs["POSITION"]).astype(np.float64)
    normals = accessor(document, binary, attrs["NORMAL"]).astype(np.float64)
    uvs = accessor(document, binary, attrs["TEXCOORD_0"]).astype(np.float64)
    triangles = accessor(document, binary, primitive["indices"]).reshape(-1, 3)
    transforms = world_matrices(document)

    if "skin" in node and "JOINTS_0" in attrs:
        skin = document["skins"][node["skin"]]
        inverse_bind = accessor(document, binary, skin["inverseBindMatrices"])
        joints = accessor(document, binary, attrs["JOINTS_0"]).astype(np.int32)
        weights = accessor(document, binary, attrs["WEIGHTS_0"]).astype(np.float64)
        skin_matrices = [transforms[joint] @ inverse_bind[i].reshape((4, 4), order="F")
                         for i, joint in enumerate(skin["joints"])]
        points = np.column_stack((positions, np.ones(len(positions))))
        posed = np.zeros_like(positions)
        posed_normals = np.zeros_like(normals)
        for column in range(4):
            for joint_index in np.unique(joints[:, column]):
                mask = joints[:, column] == joint_index
                influence = weights[mask, column, None]
                matrix = skin_matrices[int(joint_index)]
                posed[mask] += (points[mask] @ matrix.T)[:, :3] * influence
                posed_normals[mask] += (normals[mask] @ matrix[:3, :3].T) * influence
        positions, normals = posed, posed_normals
    else:
        matrix = transforms[node_index]
        positions = (np.column_stack((positions, np.ones(len(positions)))) @ matrix.T)[:, :3]
        normals = normals @ np.linalg.inv(matrix[:3, :3])

    normals /= np.maximum(np.linalg.norm(normals, axis=1, keepdims=True), 1e-10)
    OUTPUT.mkdir(parents=True, exist_ok=True)
    obj = OUTPUT / f"{name}.obj"
    lines = [f"mtllib {name}.mtl", f"o {name}"]
    lines += ["v {:.7f} {:.7f} {:.7f}".format(*point) for point in positions]
    lines += ["vt {:.7f} {:.7f}".format(uv[0], 1 - uv[1]) for uv in uvs]
    lines += ["vn {:.7f} {:.7f} {:.7f}".format(*normal) for normal in normals]
    lines.append(f"usemtl {name}")
    lines += ["f " + " ".join(f"{int(i)+1}/{int(i)+1}/{int(i)+1}" for i in face)
              for face in triangles]
    obj.write_text("\n".join(lines) + "\n", encoding="utf-8")

    mtl = [f"newmtl {name}", "Kd 1 1 1", "Ks 0 0 0", "Ns 0"]
    images = document.get("images", [])
    if images:
        image_view = document["bufferViews"][images[0]["bufferView"]]
        begin = image_view.get("byteOffset", 0)
        end = begin + image_view["byteLength"]
        texture_name = f"{name}.png"
        (OUTPUT / texture_name).write_bytes(binary[begin:end])
        mtl.append(f"map_Kd {texture_name}")
    (OUTPUT / f"{name}.mtl").write_text("\n".join(mtl) + "\n", encoding="utf-8")
    print(name, "vertices", len(positions), "triangles", len(triangles),
          "bounds", positions.min(axis=0), positions.max(axis=0))


if __name__ == "__main__":
    export("low_poly_chicken")
    export("egg")
