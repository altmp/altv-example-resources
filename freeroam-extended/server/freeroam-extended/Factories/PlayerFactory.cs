using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Async.Elements.Entities;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Freeroam_Extended.Clothes;

namespace Freeroam_Extended.Factories
{
    public partial interface IAltPlayer : IPlayer, IAsyncConvertible<IAltPlayer>
    {
        public IList<AltVehicle> Vehicles { get; set; }
        public DateTime LastVehicleSpawn { get; set; } 
        public bool GhostMode { get; set; }
        public bool EnableWeaponUsage { get; set; }
        public bool DmMode { get; set; }
        public bool NoClip { get; set; }
        public bool IsAdmin { get; set; }
        public int EventCount { get; set; }
        public string CloudID { get; set; }
        public long OutfitHash { get; set; }
        public uint Sex { get; }
        public Task RefreshClothes();
        public Task EquipOutfit(uint outfitHash);
        public void RefreshFace();
    }

    public partial class AltPlayer : AsyncPlayer, IAltPlayer
    {
        public IList<AltVehicle> Vehicles { get; set; }
        public DateTime LastVehicleSpawn { get; set; }
        public bool GhostMode { get; set; }
        public bool EnableWeaponUsage { get; set; }
        public bool DmMode { get; set; }
        public bool NoClip { get; set; }
        public bool IsAdmin { get; set; }
        public int EventCount { get; set; }
        public string CloudID { get; set; }
        public long OutfitHash { get; set; }
        public uint Sex => this.Model switch
        {
            (uint)PedModel.FreemodeMale01 => 0,
            (uint)PedModel.FreemodeFemale01 => 1,
            _ => 2
        };

        public AltPlayer(ICore server, IntPtr nativePointer, uint id) : base(server, nativePointer, id)
        {
            Vehicles = new List<AltVehicle>();
        }

        public void RefreshFace()
        {
            if (Sex == 1)
            {
                this.SetHeadBlendData(6, 21, 0, 6, 21, 0, 0.41f, 0.18f, 0.0f);
                this.SetHeadOverlay(0, 255, 1.0f);
                this.SetHeadOverlay(1, 255, 1.0f);
                this.SetHeadOverlay(2, 30, 1.0f);
                this.SetHeadOverlay(3, 255, 1.0f);
                this.SetHeadOverlay(4, 14, 1.0f);
                this.SetHeadOverlay(5, 1, 1.0f);
                this.SetHeadOverlay(6, 10, 0.85f);
                this.SetHeadOverlay(7, 255, 1.0f);
                this.SetHeadOverlay(8, 2, 1.0f);
                this.SetHeadOverlay(9, 0, 0.0f);
                this.SetHeadOverlay(10, 255, 1.0f);
                this.SetHeadOverlay(11, 255, 1.0f);
                this.SetHeadOverlay(12, 255, 1.0f);

                this.SetHeadOverlayColor(5, 2, 11, 0);
                this.SetHeadOverlayColor(8, 2, 6, 0);

                int hairs = Misc.RandomInt(1, 23);
                int hairsColor = Misc.RandomInt(1, 63);
                int hairsColor2 = Misc.RandomInt(1, 63);

                this.SetClothes(2, (ushort)hairs, 0, 0);
                this.HairColor = (byte)hairsColor;
                this.HairHighlightColor = (byte)hairsColor2;
                this.SetEyeColor(2);

                float[] featureParams = { -0.78f, 0, 0, -0.07f, 0.03f, 0, 0.07f, -0.44f, 0.07f, 0.02f, -0.95f, -0.74f, -1, -0.09f, -0.57f, 0.02f, -0.1f, -0.19f, -1, -1 };
                for (int i = 0; i < featureParams.Length; i++)
                {
                    this.SetFaceFeature((byte)i, featureParams[i]);
                }
            }
            else if (Sex == 0)
            {
                this.SetHeadBlendData(2, 21, 0, 2, 21, 0, 0.5f, 0.72f, 0.0f);
                this.SetHeadOverlay(0, 255, 1.0f);
                this.SetHeadOverlay(1, 255, 1.0f);
                this.SetHeadOverlay(2, 30, 1.0f);
                this.SetHeadOverlay(3, 255, 1.0f);
                this.SetHeadOverlay(4, 255, 1.0f);
                this.SetHeadOverlay(5, 255, 1.0f);
                this.SetHeadOverlay(6, 255, 1.0f);
                this.SetHeadOverlay(7, 255, 1.0f);
                this.SetHeadOverlay(8, 0, 0.15f);
                this.SetHeadOverlay(9, 255, 1.0f);
                this.SetHeadOverlay(10, 255, 1.0f);
                this.SetHeadOverlay(11, 255, 1.0f);
                this.SetHeadOverlay(12, 255, 1.0f);

                this.SetHeadOverlayColor(5, 2, 32, 0);
                this.SetHeadOverlayColor(8, 2, 11, 0);

                int hairs = Misc.RandomInt(1, 22);
                int hairsColor = Misc.RandomInt(1, 63);
                int hairsColor2 = Misc.RandomInt(1, 63);

                this.SetClothes(2, (ushort)hairs, 0, 0);
                this.HairColor = (byte)hairsColor;
                this.HairHighlightColor = (byte)hairsColor2;
                this.SetEyeColor(3);

                float[] featureParams = { 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1 };
                for (int i = 0; i < featureParams.Length; i++)
                {
                    this.SetFaceFeature((byte)i, featureParams[i]);
                }
            }
        }

        public async Task RefreshClothes()
        {
            if (!Misc.IsResourceLoaded("c_clothesfit"))
                return;

            if (Sex == 2)
                return;

            await ClothesFitService.DestroyPlayer(this);
            await ClothesFitService.InitPlayer(this);

            ulong[] outfits = await ClothesFitService.GetOutfitsBySex(Sex);

            int index = Misc.RandomInt(0, outfits.Length - 1);
            ulong randomElement = outfits[index];

            await ClothesFitService.Equip(this, (uint)randomElement);
        }

        public async Task EquipOutfit(uint outfitHash)
        {
            if (!Misc.IsResourceLoaded("c_clothesfit"))
                return;

            if (Sex == 2)
                return;

            await ClothesFitService.DestroyPlayer(this);
            await ClothesFitService.InitPlayer(this);

            ulong[] outfits = await ClothesFitService.GetOutfitsBySex(Sex);

            if (outfits.Contains(outfitHash))
            {
                await ClothesFitService.Equip(this, outfitHash);
            }
        }

        public new IAltPlayer ToAsync(IAsyncContext _) => this;
    }
    
    public class AltPlayerFactory : IEntityFactory<IPlayer>
    {
        public IPlayer Create(ICore core, IntPtr entityPointer, uint id)
        {
            return new AltPlayer(core, entityPointer, id);
        }
    }
}