"""Extract the cloud meshes in the supplied GLB as Unity-readable OBJ assets.

Usage: python Tools/convert_cloud_pack.py SOURCE.glb OUTPUT_DIRECTORY
The original GLB is kept alongside the generated assets for provenance.
"""

import json
import struct
import sys
from pathlib import Path


def read_accessor(document, binary, index):
    accessor = document["accessors"][index]
    view = document["bufferViews"][accessor["bufferView"]]
    components = {"SCALAR": 1, "VEC3": 3}[accessor["type"]]
    code, size = {5121: ("B", 1), 5123: ("H", 2), 5125: ("I", 4), 5126: ("f", 4)}[
        accessor["componentType"]
    ]
    format_string = "<" + code * components
    stride = view.get("byteStride", components * size)
    offset = view.get("byteOffset", 0) + accessor.get("byteOffset", 0)
    result = []
    for i in range(accessor["count"]):
        values = struct.unpack_from(format_string, binary, offset + i * stride)
        result.append(values[0] if components == 1 else values)
    return result


def convert(source, destination):
    data = Path(source).read_bytes()
    magic, version, total = struct.unpack_from("<4sII", data)
    if magic != b"glTF" or version != 2 or total != len(data):
        raise ValueError("Expected a complete glTF 2.0 binary file")
    json_length, json_type = struct.unpack_from("<II", data, 12)
    if json_type != 0x4E4F534A:
        raise ValueError("Missing GLB JSON chunk")
    document = json.loads(data[20 : 20 + json_length])
    binary_offset = 20 + json_length
    binary_length, binary_type = struct.unpack_from("<II", data, binary_offset)
    if binary_type != 0x004E4942:
        raise ValueError("Missing GLB binary chunk")
    binary = memoryview(data)[binary_offset + 8 : binary_offset + 8 + binary_length]
    destination = Path(destination)
    destination.mkdir(parents=True, exist_ok=True)

    for number, mesh in enumerate(document["meshes"]):
        if len(mesh["primitives"]) != 1 or mesh["primitives"][0].get("mode", 4) != 4:
            raise ValueError(f"Cloud mesh {number} is not a triangle mesh")
        primitive = mesh["primitives"][0]
        vertices = read_accessor(document, binary, primitive["attributes"]["POSITION"])
        normals = read_accessor(document, binary, primitive["attributes"]["NORMAL"])
        indices = read_accessor(document, binary, primitive["indices"])
        if len(indices) % 3 or len(vertices) != len(normals):
            raise ValueError(f"Cloud mesh {number} has invalid geometry")
        center = tuple((min(v[axis] for v in vertices) + max(v[axis] for v in vertices)) * 0.5 for axis in range(3))
        path = destination / f"Cloud_{number:02d}.obj"
        with path.open("w", encoding="ascii", newline="\n") as output:
            output.write(f"o Cloud_{number:02d}\n")
            for vertex in vertices:
                output.write("v {:.6f} {:.6f} {:.6f}\n".format(*(vertex[axis] - center[axis] for axis in range(3))))
            for normal in normals:
                output.write("vn {:.6f} {:.6f} {:.6f}\n".format(*normal))
            for i in range(0, len(indices), 3):
                output.write("f " + " ".join(f"{indices[j] + 1}//{indices[j] + 1}" for j in range(i, i + 3)) + "\n")
    print(f"Exported {len(document['meshes'])} cloud meshes to {destination}")


if __name__ == "__main__":
    if len(sys.argv) != 3:
        raise SystemExit(__doc__)
    convert(sys.argv[1], sys.argv[2])
