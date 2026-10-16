using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Jam.Levels.Level1
{
    internal static class L1_CozyPlayableBuilder
    {
        private const string RootPath = "Assets/Levels/Level1/";
        private const string Doll = "Assets/LowPolyInteriorsFree/Prefabs/";
        private const string Tavern = "Assets/Low Poly Tarvern Pack/Prefabs/Probs/";
        private const string Decor = "Assets/3Digitalis/LowPolyInteriorPropsPack/Prefabs/";
        private static Material wood, lightWood, plaster, iron, stone, olive, cream, glow, tile;
        private static Mesh beamMesh;
        private static Mesh salvagedBoards;
        private static Material salvagedWood;
        private static int meshIndex;
        private static Transform house;
        private static readonly float[] Floors = { -1.15f, 1.1f, 3.35f };
        private static readonly List<Collider> FloorsPhysics = new List<Collider>();

        [MenuItem("Son Denge/Level1/Oynanabilir 3D Cozy Evi Kur")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("3D evi yeniden kurmadan önce Play modunu durdurun.");
                return;
            }
            if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Level1.unity")
                throw new InvalidOperationException("Önce Level1 sahnesini açın.");
            Folder(RootPath + "Meshes");
            wood = Mat("L1_3D_WornOak", new Color(.74f,.51f,.31f), true);
            lightWood = Mat("L1_3D_HoneyOak", new Color(.95f,.76f,.48f), true);
            plaster = Mat("L1_3D_AgedPlaster", new Color(.83f,.74f,.59f));
            plaster.SetTexture("_BaseMap", PlasterTexture());
            iron = Mat("L1_3D_BlackIron", new Color(.11f,.10f,.085f));
            stone = Mat("L1_3D_OldStone", new Color(.36f,.34f,.29f));
            olive = Mat("L1_3D_Olive", new Color(.24f,.31f,.15f));
            cream = Mat("L1_3D_Linen", new Color(.83f,.72f,.48f));
            tile = Mat("L1_3D_Clay", new Color(.40f,.16f,.075f), true);
            glow = Mat("L1_3D_LanternGlow", new Color(1,.62f,.18f));
            glow.EnableKeyword("_EMISSION"); glow.SetColor("_EmissionColor",new Color(3,1.25f,.25f));
            EditorUtility.SetDirty(glow);
            beamMesh = CreateBeam(); PrepareSalvagedBoards(); meshIndex = 0;
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
                if (root.name == "[L1] World Visuals" || root.name.StartsWith("[L1] Asset Review"))
                    UnityEngine.Object.DestroyImmediate(root);
            var world = new GameObject("[L1] World Visuals").transform;
            Sprite(world,"Gökyüzü — arka plan resmi","L1_DuskSky.png",new Vector3(0,2,9),2.05f,-20);
            Sprite(world,"Dağ — arka plan resmi","L1_RockSpire.png",new Vector3(0,-4.30f,.8f),.34f,-10);
            house = Node(world,"3D Ev — ahşap yapı ve bağımsız eşyalar");
            FloorsPhysics.Clear();
            for (var level=0; level<3; level++) BuildFloor(level);
            BuildRoof();
            BuildBalcony();
            DressRooms();
            var route1 = Ladder("Alt kattan orta kata",1.55f,0);
            var route2 = Ladder("Orta kattan çatı katına",-.85f,1);
            CreateResident(new[]{route1,route2});
            house.localPosition = new Vector3(0,-.45f,0);
            var resident=house.GetComponentInChildren<L1_ResidentController>();
            resident.spawnPosition=resident.transform.position;
            var camera = Root("Main Camera").GetComponent<Camera>();
            camera.transform.SetPositionAndRotation(new Vector3(0,2,-14),Quaternion.identity);
            camera.orthographic=false; camera.fieldOfView=40;
            var controls = Node(world,"Eşya taşıma").gameObject.AddComponent<L1_PropDrag>();
            controls.sceneCamera=camera;
            var sun = Root("Directional Light").GetComponent<Light>();
            sun.transform.rotation=Quaternion.Euler(38,-28,0);
            sun.color=new Color(1,.83f,.63f);sun.intensity=.9f;sun.shadows=LightShadows.Soft;
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.30f,.36f,.49f);
            RenderSettings.ambientEquatorColor=new Color(.31f,.25f,.19f);
            RenderSettings.ambientGroundColor=new Color(.16f,.12f,.10f);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("Level1 gerçek 3D ev kuruldu: 3 kat, 2 merdiven, karakter ve taşınabilir mobilyalar.");
        }

        [MenuItem("Son Denge/Level1/Oynanabilir 3D Cozy Evi Kur", true)]
        private static bool CanBuild() => !EditorApplication.isPlayingOrWillChangePlaymode;

        private static void BuildFloor(int level)
        {
            float y=Floors[level];
            var floor=Node(house,"Kat "+(level+1));
            // Walking surface is smooth; separate weathered boards supply the silhouette.
            var physics=Node(floor,"Yürünebilir zemin"); physics.localPosition=new Vector3(0,y-.10f,0);
            var box=physics.gameObject.AddComponent<BoxCollider>();box.size=new Vector3(6.65f,.20f,2.6f);
            FloorsPhysics.Add(box);
            for(int i=0;i<22;i++)
            {
                float x=-3.23f+i*.307f;
                float hatchX=level==1?1.55f:-.85f;
                float front=level>0 && Mathf.Abs(x-hatchX)<.44f ? -.25f : -1.15f;
                Beam(floor,"Aşınmış döşeme tahtası",new Vector3(x,y-.07f,front),new Vector3(x+.015f*Mathf.Sin(i),y-.07f,1.2f),.13f,.29f,i%3==0?wood:lightWood);
            }
            Beam(floor,"Kalın ön döşeme kirişi",new Vector3(-3.5f,y-.17f,-1.24f),new Vector3(3.5f,y-.13f,-1.24f),.22f,.30f,wood);
            Beam(floor,"Arka döşeme kirişi",new Vector3(-3.4f,y-.15f,1.2f),new Vector3(3.4f,y-.15f,1.2f),.23f,.26f,wood);
            SalvagedPatch(floor,new Vector3(-2.1f,y+.004f,.50f),new Vector3(2.1f,.045f,1.30f));
            SalvagedPatch(floor,new Vector3(.2f,y+.004f,.50f),new Vector3(2.0f,.045f,1.30f));
            // Small iron bands and exposed pegged beam ends.
            for(int i=0;i<8;i++)
            {
                float x=-3.25f+i*.93f;
                Beam(floor,"Zemin altı kiriş ucu",new Vector3(x,y-.3f,-1.4f),new Vector3(x,y-.3f,1.25f),.17f,.19f,wood);
                Stud(floor,new Vector3(x,y-.15f,-1.405f));
            }
            var back=Node(floor,"Eskimiş arka duvar");
            // Real arched opening; geometry stops at the opening boundary.
            float windowX=level==1?-2.25f:-2.20f;
            WallPanel(back,-3.3f,windowX-.52f,y,y+2.2f);
            WallPanel(back,windowX+.52f,3.3f,y,y+2.2f);
            WallPanel(back,windowX-.52f,windowX+.52f,y,y+.58f);
            var archTop=new List<Vector3>();
            for(int i=0;i<=16;i++)
            {
                float x=windowX-.52f+i*1.04f/16;
                float arch=y+1.46f+Mathf.Sqrt(Mathf.Max(0,.52f*.52f-(x-windowX)*(x-windowX)));
                archTop.Add(new Vector3(x,arch,1.18f));archTop.Add(new Vector3(x,y+2.2f,1.18f));
            }
            Strip(back,"Duvar kemeri",archTop,plaster);
            Arch(back,windowX,y+.58f,.52f,.88f,wood);
            Beam(back,"Pencere dikmesi",new Vector3(windowX,y+.58f,1.03f),new Vector3(windowX,y+1.97f,1.03f),.045f,.065f,lightWood);
            Beam(back,"Pencere çaprazı",new Vector3(windowX-.5f,y+1.18f,1.03f),new Vector3(windowX+.5f,y+1.18f,1.03f),.04f,.065f,lightWood);
            Beam(back,"Pencere pervazı",new Vector3(windowX-.65f,y+.55f,.99f),new Vector3(windowX+.65f,y+.55f,.99f),.12f,.28f,wood);
            // Worn timber dado, masonry and cracks break up the plaster.
            for(int row=0;row<2;row++) for(int i=0;i<20;i++)
            {
                float x=-3.2f+i*.33f;
                if(i%5==level || (row==1 && i%3==0)) continue;
                Beam(back,"Sıva altından çıkan taş",new Vector3(x,y+.13f+row*.19f,1.08f),new Vector3(x+.28f,y+.14f+row*.19f,1.08f),.15f,.13f,stone);
            }
            for(int i=0;i<8;i++)
            {
                float x=-1.5f+i*.55f;
                float cy=y+.65f+(i%3)*.42f;
                Beam(back,"Sıva çatlağı",new Vector3(x,cy,1.075f),new Vector3(x+.09f,cy+.16f,1.075f),.009f,.012f,stone);
                Beam(back,"Sıva çatlağı",new Vector3(x+.09f,cy+.16f,1.075f),new Vector3(x+.045f,cy+.24f,1.075f),.008f,.012f,stone);
            }
            for(int s=-1;s<=1;s+=2)
            {
                float x=s*3.26f;
                Beam(floor,"Eğri ana ahşap direk",new Vector3(x,y-.25f,-.95f),new Vector3(x+s*.07f,y+2.20f,-.91f),.22f,.25f,wood);
                Beam(floor,"Arka ana direk",new Vector3(x,y,1.08f),new Vector3(x+s*.05f,y+2.22f,1.08f),.19f,.23f,wood);
                Beam(floor,"Çapraz ahşap payanda",new Vector3(x,y+.2f,-.92f),new Vector3(x-s*.51f,y+.84f,-.92f),.13f,.15f,lightWood);
                Beam(floor,"Tavan köşe desteği",new Vector3(x,y+1.62f,-.92f),new Vector3(x-s*.58f,y+2.15f,-.92f),.13f,.15f,wood);
                Stud(floor,new Vector3(x,y+.22f,-1.08f));
                var wall=Node(floor,"Yan duvar çarpışması");wall.localPosition=new Vector3(s*3.47f,y+1.1f,.1f);
                var collider=wall.gameObject.AddComponent<BoxCollider>();collider.size=new Vector3(.16f,2.2f,2.6f);
                for(int plank=0;plank<5;plank++)
                    Beam(floor,"Yan duvar eski ahşap kaplama",new Vector3(x,y+.05f,.25f+plank*.20f),new Vector3(x+s*.04f,y+2.15f,.25f+plank*.20f),.19f,.12f,plank%2==0?wood:lightWood);
            }
            Beam(back,"Yatay duvar ahşabı",new Vector3(-1.55f,y+1.25f,1.03f),new Vector3(3.28f,y+1.29f,1.03f),.085f,.10f,wood);
            Door(back,2.50f,y+.03f);
            for(int l=0;l<2;l++) Lamp(floor,new Vector3(l==0?-.5f:2.1f,y+1.77f,.25f));
            Shelf(floor,new Vector3(level==1?.3f:-.25f,y+1.3f,.86f),1.2f);
            Shelf(floor,new Vector3(level==1?.45f:-.15f,y+1.68f,.86f),1.05f);
            Prop(Decor+"P_Plant.prefab",floor,"Raf üzerindeki sarmaşık",new Vector3(.65f,y+1.91f,.8f),new Vector3(.32f,.40f,.26f));
        }

        private static void BuildRoof()
        {
            var roof=Node(house,"Asimetrik kiremit çatı");
            for(int row=0;row<4;row++) for(int i=0;i<24;i++)
            {
                float x=-3.58f+i*.31f;
                float y=5.42f+(x+3.58f)*.145f;
                Beam(roof,"Düzensiz kil kiremit",new Vector3(x,y+row*.045f,-1.35f+row*.66f),new Vector3(x+.012f,y+row*.045f,-.58f+row*.66f),.09f,.34f,i%4==0?wood:tile);
            }
            Beam(roof,"Eğimli ön çatı saçağı",new Vector3(-3.8f,5.36f,-1.45f),new Vector3(3.8f,6.46f,-1.45f),.23f,.30f,wood);
            Beam(roof,"İç çatı kirişi",new Vector3(-3.7f,5.23f,-1.01f),new Vector3(3.7f,6.30f,-1.01f),.14f,.18f,lightWood);
            for(int i=0;i<10;i++)
            {
                float x=-3.4f+i*.75f;float y=5.4f+(x+3.58f)*.145f;
                Beam(roof,"Çatı merteği",new Vector3(x,y-.10f,-1.52f),new Vector3(x,y+.07f,1.3f),.12f,.16f,lightWood);
            }
            var verts=new List<Vector3>{new Vector3(-3.3f,5.40f,1.18f),new Vector3(3.3f,5.40f,1.18f),new Vector3(3.3f,6.55f,1.18f),new Vector3(-3.3f,5.61f,1.18f)};
            Polygon(roof,"Üst çatı sıvası",verts,plaster);
            Beam(roof,"Çatıya uzanan sağ direk",new Vector3(3.33f,5.35f,-.91f),new Vector3(3.35f,6.28f,-.91f),.22f,.25f,wood);
            Beam(roof,"Üst çatı duvar çaprazı",new Vector3(-.1f,5.55f,1.04f),new Vector3(2.5f,6.22f,1.04f),.10f,.12f,wood);
            Prop(Doll+"01_Structure/SM_LPI_Struct_Column_01.prefab",roof,"Baca",new Vector3(-2.6f,6.15f,.7f),new Vector3(.42f,1.1f,.45f),stone);
            for(int row=0;row<6;row++) Beam(roof,"Baca tuğla sırası",new Vector3(-2.88f,5.7f+row*.18f,.7f),new Vector3(-2.32f,5.7f+row*.18f,.7f),.13f,.56f,stone);
        }

        private static void BuildBalcony()
        {
            var balcony=Node(house,"Ahşap balkon ve vinç");
            for(int level=1;level<3;level++)
            {
                float y=Floors[level];
                Beam(balcony,"Balkon döşemesi",new Vector3(3.3f,y-.12f,-.6f),new Vector3(4.1f,y-.12f,-.6f),.16f,1.6f,lightWood);
                for(int i=0;i<4;i++) Beam(balcony,"Balkon korkuluğu",new Vector3(3.35f+i*.23f,y,-1.28f),new Vector3(3.35f+i*.23f,y+.68f,-1.28f),.055f,.07f,wood);
                Beam(balcony,"Balkon küpeştesi",new Vector3(3.28f,y+.7f,-1.28f),new Vector3(4.12f,y+.74f,-1.28f),.08f,.10f,wood);
                Prop("Assets/EmaceArt/Fences 2026/Prefabs/Fence/Beam/PRE/EA03_Fence_Beam_01a_PRE.prefab",balcony,"Roadside eski ahşap korkuluk",new Vector3(3.75f,y+.36f,-1.2f),new Vector3(.85f,.70f,.14f));
                Beam(balcony,"Balkon payandası",new Vector3(3.25f,y-.8f,-.4f),new Vector3(4,y-.12f,-.4f),.14f,.16f,wood);
            }
            Beam(balcony,"Vinç kolu",new Vector3(3.12f,5.45f,.2f),new Vector3(4.55f,5.7f,.2f),.16f,.2f,wood);
            Beam(balcony,"Vinç çaprazı",new Vector3(3.25f,4.6f,.2f),new Vector3(4.4f,5.65f,.2f),.12f,.15f,wood);
            Beam(balcony,"Vinç halatı",new Vector3(4.5f,5.7f,.2f),new Vector3(4.5f,4.45f,.2f),.024f,.024f,iron);
            Prop(Tavern+"Wooden Chest.prefab",balcony,"Vinçte asılı sandık",new Vector3(4.5f,4.22f,.2f),new Vector3(.6f,.48f,.48f));
        }

        private static void DressRooms()
        {
            var p=Node(house,"Bağımsız 3D mobilyalar");
            Movable(Doll+"12_Bathroom/SM_LPI_Bath_Tub_01.prefab",p,"Küvet",-2.5f,0,new Vector3(1.05f,.58f,.7f),5);
            var counter=Movable(Tavern+"Bar Counter.prefab",p,"Mutfak tezgâhı",-.55f,0,new Vector3(1.3f,.73f,.65f),4);
            Prop(Tavern+"Fat Mead Jug.prefab",counter,"Tezgahtaki sürahi",new Vector3(-.32f,.48f,0),new Vector3(.20f,.25f,.20f));
            Prop(Tavern+"Wooden Serving Plate.prefab",counter,"Ahşap servis tabağı",new Vector3(.25f,.39f,0),new Vector3(.33f,.05f,.28f));
            Movable(Tavern+"Standard Stool.prefab",p,"Tabure",.65f,0,new Vector3(.4f,.5f,.4f),1);
            Movable(Tavern+"Standing Barrel.prefab",p,"Ahşap fıçı",2.65f,0,new Vector3(.55f,.72f,.55f),3);
            var sofa=Movable(Doll+"04_Seating/SM_LPI_Seat_Sofa_01.prefab",p,"Yeşil kanepe",-2.1f,1,new Vector3(1.45f,.78f,.68f),3,olive);
            Prop(Doll+"16_Textiles/SM_LPI_Text_Cushion_01.prefab",sofa,"Keten yastık",new Vector3(-.35f,.12f,-.18f),new Vector3(.30f,.24f,.13f),cream);
            Movable(Tavern+"Wooden Chest.prefab",p,"Ahşap sandık",.30f,1,new Vector3(.8f,.55f,.5f),2);
            var table=Movable(Tavern+"Dining Table.prefab",p,"Yemek masası",1.75f,1,new Vector3(1.1f,.7f,.65f),2);
            Prop(Tavern+"Single Candle 3.prefab",table,"Masa mumu",new Vector3(-.28f,.46f,.02f),new Vector3(.13f,.23f,.13f));
            Prop(Tavern+"Full Foam Tankard.prefab",table,"Masa kupası",new Vector3(.25f,.43f,-.02f),new Vector3(.15f,.17f,.15f));
            Movable(Tavern+"Backing Chair.prefab",p,"Ahşap sandalye",2.75f,1,new Vector3(.42f,.78f,.43f),1);
            var bed=Movable(Doll+"07_Bedroom/SM_LPI_Bed_Double_01.prefab",p,"Ahşap yatak",-2.1f,2,new Vector3(1.65f,.72f,.9f),4);
            Prop(Doll+"16_Textiles/SM_LPI_Text_Throw_01.prefab",bed,"Yatak örtüsü",new Vector3(.1f,.10f,-.02f),new Vector3(1.05f,.09f,.72f),olive);
            Movable(Doll+"07_Bedroom/SM_LPI_Bed_Nightstand_01.prefab",p,"Komodin",-.65f,2,new Vector3(.5f,.57f,.45f),1);
            var bookcase=Movable(Doll+"06_Storage/SM_LPI_Storage_Bookcase_03.prefab",p,"Kitaplık",1.3f,2,new Vector3(.85f,1.55f,.4f),4);
            for(int shelf=0;shelf<3;shelf++) Prop(Decor+"P_Books_02.prefab",bookcase,"Kitaplar",new Vector3(-.1f,.10f+shelf*.25f,-.10f),new Vector3(.5f,.18f,.18f));
            Movable(Tavern+"Wooden Chest.prefab",p,"Çeyiz sandığı",2.65f,2,new Vector3(.7f,.5f,.48f),2);
            for(int level=0;level<3;level++)
            {
                Prop(Decor+"P_Plant.prefab",p,"Saksı bitkisi",new Vector3(3.0f,Floors[level]+.25f,-.05f),new Vector3(.30f,.50f,.30f));
                Prop(Doll+"16_Textiles/SM_LPI_Text_RugRect_01.prefab",p,"Dokuma kilim",new Vector3(-1.5f,Floors[level]+.018f,-.15f),new Vector3(1.9f,.025f,1.2f),level==0?cream:olive);
            }
        }

        private static Transform Movable(string path,Transform parent,string name,float x,int floor,Vector3 size,float mass,Material material=null)
        {
            var root=Node(parent,name);root.localPosition=new Vector3(x,Floors[floor]+size.y*.5f+.03f,.25f);
            Prop(path,root,"Model",Vector3.zero,size,material);
            var box=root.gameObject.AddComponent<BoxCollider>();box.size=size;
            var body=root.gameObject.AddComponent<Rigidbody>();body.mass=mass;body.interpolation=RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;body.linearDamping=.7f;body.angularDamping=2f;
            body.constraints=RigidbodyConstraints.FreezePositionZ|RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationY;
            var movable=root.gameObject.AddComponent<L1_MovableProp>();movable.displayName=name;movable.itemId="l1-"+name;
            return root;
        }

        private static L1_ClimbRoute Ladder(string name,float x,int level)
        {
            float low=Floors[level],high=Floors[level+1];
            var root=Node(house,name);
            Prop(Doll+"20_Utility/SM_LPI_Util_Ladder_01.prefab",root,"Gerçek ahşap merdiven modeli",new Vector3(x,(low+high)*.5f,-.66f),new Vector3(.55f,2.4f,.18f),wood);
            var route=root.gameObject.AddComponent<L1_ClimbRoute>();
            route.bottom=Node(root,"Alt giriş");route.bottom.position=new Vector3(x,low+.43f,-.96f);
            route.top=Node(root,"Üst giriş");route.top.position=new Vector3(x,high+.43f,-.96f);
            route.landing=Node(root,"Üst çıkış");route.landing.position=new Vector3(x+.65f,high+.43f,-.96f);
            route.upperFloor=FloorsPhysics[level+1];
            route.hatch=Node(root,"Açılır merdiven kapağı");route.hatch.position=new Vector3(x,high+.025f,-.66f);
            Beam(route.hatch,"Kapak tahtası",new Vector3(-.34f,0,0),new Vector3(.34f,0,0),.06f,.7f,wood);
            return route;
        }

        private static void CreateResident(L1_ClimbRoute[] routes)
        {
            var root=Node(house,"Oyuncu — evin sakini");root.position=new Vector3(-1.45f,Floors[0]+.43f,-.96f);root.tag="Player";
            var capsule=root.gameObject.AddComponent<CapsuleCollider>();capsule.height=.84f;capsule.radius=.18f;
            var body=root.gameObject.AddComponent<Rigidbody>();body.mass=1;body.constraints=RigidbodyConstraints.FreezePositionZ|RigidbodyConstraints.FreezeRotation;
            body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            var visual=Node(root,"3D sakin görünümü");
            Ellipsoid(visual,"Kaban",new Vector3(0,-.08f,0),new Vector3(.38f,.49f,.29f),olive);
            Ellipsoid(visual,"Yüz",new Vector3(0,.23f,-.03f),new Vector3(.29f,.27f,.25f),cream);
            Ellipsoid(visual,"Şapka kenarı",new Vector3(0,.35f,0),new Vector3(.48f,.065f,.34f),olive);
            Beam(visual,"Sivri ahşap renkli şapka",new Vector3(0,.35f,0),new Vector3(-.10f,.65f,.01f),.17f,.21f,olive);
            Ellipsoid(visual,"Burun",new Vector3(0,.23f,-.17f),new Vector3(.09f,.08f,.10f),cream);
            Ellipsoid(visual,"Sol göz",new Vector3(-.065f,.27f,-.145f),Vector3.one*.035f,iron);
            Ellipsoid(visual,"Sağ göz",new Vector3(.065f,.27f,-.145f),Vector3.one*.035f,iron);
            var left=Ellipsoid(visual,"Sol çizme",new Vector3(-.105f,-.35f,-.035f),new Vector3(.14f,.18f,.22f),wood);
            var right=Ellipsoid(visual,"Sağ çizme",new Vector3(.105f,-.35f,-.035f),new Vector3(.14f,.18f,.22f),wood);
            Ellipsoid(visual,"Sol kol",new Vector3(-.23f,-.06f,0),new Vector3(.12f,.32f,.13f),olive);
            Ellipsoid(visual,"Sağ kol",new Vector3(.23f,-.06f,0),new Vector3(.12f,.32f,.13f),olive);
            var player=root.gameObject.AddComponent<L1_ResidentController>();player.ladders=routes;player.visual=visual;player.leftFoot=left;player.rightFoot=right;player.spawnPosition=root.position;
        }

        private static void Shelf(Transform parent,Vector3 pos,float width)
        {
            Prop(Decor+"P_Shelf.prefab",parent,"Ahşap duvar rafı",pos,new Vector3(width,.09f,.28f),wood);
            for(int i=0;i<4;i++) Prop(Tavern+(i%2==0?"Fat Mead Jug.prefab":"Thin Wine Bottle 1.prefab"),parent,"Rafta seramik ve şişeler",pos+new Vector3(-width*.35f+i*width*.23f,.15f,-.02f),new Vector3(.14f,.24f,.14f));
        }

        private static void Lamp(Transform parent,Vector3 pos)
        {
            var lamp=Node(parent,"Cozy Tavern lamba ve fener");lamp.localPosition=pos;
            Prop(Tavern+"Iron Torch Bracket.prefab",lamp,"Tavern demir lamba",new Vector3(0,0,.33f),new Vector3(.23f,.48f,.25f));
            Prop(Doll+"14_Decor/SM_LPI_Decor_Lantern_01.prefab",lamp,"Asılı fener",new Vector3(0,-.05f,-.05f),new Vector3(.22f,.40f,.22f));
            Beam(lamp,"Fener askısı",new Vector3(0,.19f,-.05f),new Vector3(0,.42f,-.05f),.018f,.018f,iron);
            Ellipsoid(lamp,"Amber ışık çekirdeği",new Vector3(0,-.04f,-.05f),new Vector3(.09f,.17f,.09f),glow);
            var light=Node(lamp,"Sıcak fener ışığı").gameObject.AddComponent<Light>();
            light.transform.localPosition=new Vector3(0,-.06f,-.28f);light.type=LightType.Point;light.color=new Color(1,.65f,.34f);light.intensity=1.25f;light.range=3.1f;light.shadows=LightShadows.None;
        }

        private static void Door(Transform parent,float x,float y)
        {
            Arch(parent,x,y,.38f,1.05f,wood);
            for(int i=0;i<6;i++)
            {
                float dx=-.31f+i*.125f;float h=1.05f+Mathf.Sqrt(Mathf.Max(0,.36f*.36f-dx*dx));
                Beam(parent,"Eski kapı tahtası",new Vector3(x+dx,y,1.01f),new Vector3(x+dx,y+h,1.01f),.116f,.08f,lightWood);
            }
            Beam(parent,"Kapı demiri",new Vector3(x-.33f,y+.35f,.94f),new Vector3(x+.33f,y+.35f,.94f),.045f,.035f,iron);
            Beam(parent,"Kapı demiri",new Vector3(x-.33f,y+.98f,.94f),new Vector3(x+.33f,y+.98f,.94f),.045f,.035f,iron);
            Stud(parent,new Vector3(x+.22f,y+.73f,.91f));
        }

        private static void Arch(Transform parent,float x,float y,float radius,float stem,Material mat)
        {
            Beam(parent,"Kemer sol direği",new Vector3(x-radius,y,1.04f),new Vector3(x-radius,y+stem,1.04f),.11f,.17f,mat);
            Beam(parent,"Kemer sağ direği",new Vector3(x+radius,y,1.04f),new Vector3(x+radius,y+stem,1.04f),.11f,.17f,mat);
            for(int i=0;i<14;i++)
            {
                float a=i*Mathf.PI/14,b=(i+1)*Mathf.PI/14;
                Beam(parent,"Oyma kemer parçası",new Vector3(x+Mathf.Cos(a)*radius,y+stem+Mathf.Sin(a)*radius,1.04f),new Vector3(x+Mathf.Cos(b)*radius,y+stem+Mathf.Sin(b)*radius,1.04f),.105f,.17f,mat);
            }
        }

        private static void WallPanel(Transform parent,float left,float right,float bottom,float top)
        {
            Polygon(parent,"Sıvalı duvar",new List<Vector3>{new Vector3(left,bottom,1.18f),new Vector3(right,bottom,1.18f),new Vector3(right,top,1.18f),new Vector3(left,top,1.18f)},plaster);
        }
        private static void Polygon(Transform parent,string name,List<Vector3> points,Material material)
        {
            var triangles=new List<int>();for(int i=1;i<points.Count-1;i++){triangles.Add(0);triangles.Add(i+1);triangles.Add(i);}
            MeshObject(parent,name,points.ToArray(),triangles.ToArray(),material);
        }
        private static void Strip(Transform parent,string name,List<Vector3> points,Material material)
        {
            var triangles=new List<int>();for(int i=0;i<points.Count-2;i+=2){triangles.AddRange(new[]{i,i+1,i+2,i+1,i+3,i+2});}
            MeshObject(parent,name,points.ToArray(),triangles.ToArray(),material);
        }
        private static void MeshObject(Transform parent,string name,Vector3[] vertices,int[] triangles,Material material)
        {
            string path=RootPath+"Meshes/L1_Wall_"+(meshIndex++)+".asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);bool fresh=mesh==null;if(fresh)mesh=new Mesh();mesh.Clear();mesh.vertices=vertices;mesh.triangles=triangles;
            var uv=new Vector2[vertices.Length];for(int i=0;i<uv.Length;i++)uv[i]=new Vector2(vertices[i].x*.6f,vertices[i].y*.6f);mesh.uv=uv;
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            if(fresh)AssetDatabase.CreateAsset(mesh,path);else EditorUtility.SetDirty(mesh);
            var go=Node(parent,name);go.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;go.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
        private static void Beam(Transform parent,string name,Vector3 a,Vector3 b,float width,float depth,Material material)
        {
            var go=Node(parent,name);go.localPosition=(a+b)*.5f;
            go.localRotation=Quaternion.FromToRotation(Vector3.right,(b-a).normalized);
            // Depth follows the local cross section; floor planks use their long axis in Z.
            go.localScale=new Vector3(Vector3.Distance(a,b),width,depth);
            go.gameObject.AddComponent<MeshFilter>().sharedMesh=beamMesh;go.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
        private static Mesh CreateBeam()
        {
            string path=RootPath+"Meshes/L1_HandHewnBeam.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh!=null)return mesh;
            mesh=new Mesh{name="Hand hewn bevelled timber"};var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();
            var ring=new[]{new Vector2(-.36f,-.5f),new Vector2(.36f,-.5f),new Vector2(.5f,-.36f),new Vector2(.5f,.36f),new Vector2(.36f,.5f),new Vector2(-.36f,.5f),new Vector2(-.5f,.36f),new Vector2(-.5f,-.36f)};
            for(int r=0;r<4;r++)for(int j=0;j<8;j++){float jitter=1+Mathf.Sin(r*7+j*3)*.055f;v.Add(new Vector3(r/3f-.5f,ring[j].x*jitter+Mathf.Sin(r*2)*.035f,ring[j].y*jitter));uv.Add(new Vector2(r/3f,j/7f));}
            for(int r=0;r<3;r++)for(int j=0;j<8;j++){int a=r*8+j,b=r*8+(j+1)%8;t.AddRange(new[]{a,b,a+8,b,b+8,a+8});}
            for(int j=1;j<7;j++){t.AddRange(new[]{0,j+1,j,24,24+j,24+j+1});}
            mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        private static Transform Ellipsoid(Transform parent,string name,Vector3 position,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=material;return go.transform;
        }
        private static void Stud(Transform parent,Vector3 pos)=>Ellipsoid(parent,"Dövme demir çivi",pos,new Vector3(.065f,.065f,.035f),iron);

        private static GameObject Prop(string path,Transform parent,string name,Vector3 center,Vector3 size,Material material=null)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(source==null)throw new InvalidOperationException(path);
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(source,parent);instance.name=name;instance.transform.localPosition=Vector3.zero;instance.transform.localRotation=Quaternion.identity;instance.transform.localScale=Vector3.one;
            foreach(var c in instance.GetComponentsInChildren<Collider>()) c.enabled=false;
            var renderers=instance.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            instance.transform.localScale=new Vector3(size.x/Mathf.Max(.001f,bounds.size.x),size.y/Mathf.Max(.001f,bounds.size.y),size.z/Mathf.Max(.001f,bounds.size.z));
            bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            instance.transform.position+=parent.TransformPoint(center)-bounds.center;
            if(material!=null)foreach(var r in renderers){var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)mats[i]=material;r.sharedMaterials=mats;}
            return instance;
        }
        private static void PrepareSalvagedBoards()
        {
            string meshPath=RootPath+"Meshes/L1_SalvagedPlanks.asset";
            salvagedBoards=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if(salvagedBoards==null)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Ata Khani/Modular Sewer Props/Prefabs/Wooden_Plank_01.prefab");
                if(prefab==null)throw new InvalidOperationException("Eskitilmiş tahta kaynağı bulunamadı.");
                salvagedBoards=UnityEngine.Object.Instantiate(prefab.GetComponentInChildren<MeshFilter>().sharedMesh);
                salvagedBoards.name="Salvaged wood — Ata Khani";AssetDatabase.CreateAsset(salvagedBoards,meshPath);
            }
            string texturePath=RootPath+"Art/L1_SalvagedWood.png";
            if(AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath)==null)
            {
                AssetDatabase.CopyAsset("Assets/Ata Khani/Modular Sewer Props/Textures/Trim_Wood_basecolor.png",texturePath);
                var importer=(TextureImporter)AssetImporter.GetAtPath(texturePath);importer.maxTextureSize=2048;importer.SaveAndReimport();
            }
            salvagedWood=Mat("L1_3D_SalvagedWood",new Color(.85f,.70f,.49f));
            salvagedWood.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
        }
        private static void SalvagedPatch(Transform parent,Vector3 position,Vector3 size)
        {
            var root=Node(parent,"Asset — aşınmış ahşap döşeme");var b=salvagedBoards.bounds;
            var scale=new Vector3(size.x/b.size.x,size.y/b.size.y,size.z/b.size.z);
            root.localScale=scale;root.localPosition=position-Vector3.Scale(b.center,scale);
            root.gameObject.AddComponent<MeshFilter>().sharedMesh=salvagedBoards;
            root.gameObject.AddComponent<MeshRenderer>().sharedMaterial=salvagedWood;
        }
        private static Texture2D PlasterTexture()
        {
            string path=RootPath+"Materials/L1_3D_PlasterGrain.asset";
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(texture!=null)return texture;
            texture=new Texture2D(256,256,TextureFormat.RGB24,true){name="Aged plaster grain",wrapMode=TextureWrapMode.Repeat};
            var pixels=new Color[256*256];
            for(int y=0;y<256;y++)for(int x=0;x<256;x++)
            {
                float n=Mathf.PerlinNoise(x*.042f,y*.042f)*.55f+Mathf.PerlinNoise(x*.21f,y*.21f)*.25f+Mathf.PerlinNoise(x*.8f,y*.8f)*.2f;
                float shade=Mathf.Lerp(.58f,1f,n);
                pixels[y*256+x]=new Color(shade,shade*.98f,shade*.93f);
            }
            texture.SetPixels(pixels);texture.Apply();AssetDatabase.CreateAsset(texture,path);return texture;
        }
        private static Material Mat(string name,Color color,bool texture=false)
        {
            var path=RootPath+"Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
            mat.color=color;mat.SetFloat("_Smoothness",.10f);mat.SetFloat("_Cull",0);
            if(texture)mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(RootPath+"Art/L1_OakFloor_Diffuse.tga"));
            EditorUtility.SetDirty(mat);return mat;
        }
        private static Transform Node(Transform parent,string name){var go=new GameObject(name);go.transform.SetParent(parent,false);return go.transform;}
        private static GameObject Root(string name){foreach(var r in EditorSceneManager.GetActiveScene().GetRootGameObjects())if(r.name==name)return r;throw new InvalidOperationException(name);}
        private static void Folder(string path){path=path.TrimEnd('/');if(AssetDatabase.IsValidFolder(path))return;int i=path.LastIndexOf('/');AssetDatabase.CreateFolder(path.Substring(0,i),path.Substring(i+1));}
        private static void Sprite(Transform parent,string name,string file,Vector3 pos,float scale,int order){var go=Node(parent,name);go.localPosition=pos;go.localScale=Vector3.one*scale;var r=go.gameObject.AddComponent<SpriteRenderer>();r.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(RootPath+"Art/"+file);r.sortingOrder=order;}

        [MenuItem("Son Denge/Level1/3D Ev Onizleme Kaydet")]
        public static void Capture()
        {
            var camera=Root("Main Camera").GetComponent<Camera>();var old=camera.targetTexture;var active=RenderTexture.active;
            var rt=RenderTexture.GetTemporary(1920,1080,24);var image=new Texture2D(1920,1080,TextureFormat.RGB24,false);
            try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1920,1080),0,0);image.Apply();File.WriteAllBytes(RootPath+"Art/L1_Playable3D_Preview.png",image.EncodeToPNG());AssetDatabase.ImportAsset(RootPath+"Art/L1_Playable3D_Preview.png");}
            finally{camera.targetTexture=old;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(image);}
        }
    }
}
