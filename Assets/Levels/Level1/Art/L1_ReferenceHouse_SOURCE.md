# Level1 referans ev — kaynak ve üretim kaydı

## Dosyalar

- `L1_ReferenceHouse_Empty.png`: Kullanıcının sağladığı `ChatGPT Görseli 16 Eki 2026 23_37_03-1.png` dosyasının kopyası; 1086×1448, şeffaf PNG.
- `L1_ReferenceHouse_Furnished.png`: Yukarıdaki evin mobilyalı düzenlemesi; 1086×1448, şeffaf PNG. İkinci stil referansı kullanıcının `Karikatür Ev İç Mekân Konsept Panosu (1).png` görselidir.
- `L1_ReferenceHouse_Preview.png`: Unity Level1 kamerasından alınan 1920×1080 sahne görüntüsü.

Araç: yerleşik `image_gen.imagegen`, mevcut görsel düzenleme, `transparent_background: true`. Yeni üçüncü taraf varlık indirilmedi. Kullanıcının verdiği kaynaklar için ayrıca bir Asset Store lisansı iddia edilmez.

Ev sabit yan kamera için bir sprite olarak kullanılır. Mobilyalar, fenerler ve ahşap detaylar görüntüye işlenmiştir. Ayrı 3D zeminler ve merdiven alanları vardır; tam 3D ev modeli veya taşınabilir mobilya seti değildir. Mobilyasız özgün varyant da korunmuştur.

## Kullanılan nihai istem

```text
Use case: precise-object-edit / compositing. Asset: transparent RGBA house facade for a fixed-side-view Unity game. Image 1 is the EDIT TARGET: preserve its exact three-storey timber house structure, silhouette, camera angle, floor heights, crooked roof, weathered hand-hewn beams, cracked plaster, arched windows, hanging amber iron lanterns, balconies and crane. Keep the complete house visible with a small transparent margin. Preserve genuine transparency around the building and through the open windows. Image 2 is ONLY the furniture and material style reference; do not reproduce its collage layout, text or characters. Furnish Image 1 sparingly in that exact rich cozy weathered style: attic/upper floor a wooden bed with olive blanket and cream pillow left, small bedside table; middle floor olive upholstered old sofa left and a wooden storage chest toward right, preserving an open walking strip; lowest floor a small white old bathtub left and a worn kitchen cabinet with ceramic jugs right. Add one slim rustic wood ladder in middle floor near center, joining the middle and upper floor through a plausible small floor hatch. Keep existing bottom-floor ladders. Add a few bottles/books and small potted plants on the existing shelves. Warm amber light, worn rounded/chipped timber, fine wood grain and cracks, warm cream plaster with exposed old stones. No characters, no text, no UI, no border, no ground, no sky. Do not simplify into geometric cubes, do not redesign the architecture, do not add floors. Maintain the same portrait aspect ratio and near-identical framing as Image 1. This is a high fidelity edit of Image 1.
```
