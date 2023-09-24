using Freeroam_Extended.Clothes;
using Freeroam_Extended.Factories;

namespace Freeroam_Extended;

public class AppearanceController
{
    
        public static void RefreshFace(IAltPlayer player)
        {
            if (player.Sex == 1)
            {
                player.SetHeadBlendData(6, 21, 0, 6, 21, 0, 0.41f, 0.18f, 0.0f);
                player.SetHeadOverlay(0, 255, 1.0f);
                player.SetHeadOverlay(1, 255, 1.0f);
                player.SetHeadOverlay(2, 30, 1.0f);
                player.SetHeadOverlay(3, 255, 1.0f);
                player.SetHeadOverlay(4, 14, 1.0f);
                player.SetHeadOverlay(5, 1, 1.0f);
                player.SetHeadOverlay(6, 10, 0.85f);
                player.SetHeadOverlay(7, 255, 1.0f);
                player.SetHeadOverlay(8, 2, 1.0f);
                player.SetHeadOverlay(9, 0, 0.0f);
                player.SetHeadOverlay(10, 255, 1.0f);
                player.SetHeadOverlay(11, 255, 1.0f);
                player.SetHeadOverlay(12, 255, 1.0f);

                player.SetHeadOverlayColor(5, 2, 11, 0);
                player.SetHeadOverlayColor(8, 2, 6, 0);

                int hairs = Misc.RandomInt(1, 23);
                int hairsColor = Misc.RandomInt(1, 63);
                int hairsColor2 = Misc.RandomInt(1, 63);

                player.SetClothes(2, (ushort)hairs, 0, 0);
                player.HairColor = (byte)hairsColor;
                player.HairHighlightColor = (byte)hairsColor2;
                player.SetEyeColor(2);

                float[] featureParams = { -0.78f, 0, 0, -0.07f, 0.03f, 0, 0.07f, -0.44f, 0.07f, 0.02f, -0.95f, -0.74f, -1, -0.09f, -0.57f, 0.02f, -0.1f, -0.19f, -1, -1 };
                for (int i = 0; i < featureParams.Length; i++)
                {
                    player.SetFaceFeature((byte)i, featureParams[i]);
                }
            }
            else if (player.Sex == 0)
            {
                player.SetHeadBlendData(2, 21, 0, 2, 21, 0, 0.5f, 0.72f, 0.0f);
                player.SetHeadOverlay(0, 255, 1.0f);
                player.SetHeadOverlay(1, 255, 1.0f);
                player.SetHeadOverlay(2, 30, 1.0f);
                player.SetHeadOverlay(3, 255, 1.0f);
                player.SetHeadOverlay(4, 255, 1.0f);
                player.SetHeadOverlay(5, 255, 1.0f);
                player.SetHeadOverlay(6, 255, 1.0f);
                player.SetHeadOverlay(7, 255, 1.0f);
                player.SetHeadOverlay(8, 0, 0.15f);
                player.SetHeadOverlay(9, 255, 1.0f);
                player.SetHeadOverlay(10, 255, 1.0f);
                player.SetHeadOverlay(11, 255, 1.0f);
                player.SetHeadOverlay(12, 255, 1.0f);

                player.SetHeadOverlayColor(5, 2, 32, 0);
                player.SetHeadOverlayColor(8, 2, 11, 0);

                int hairs = Misc.RandomInt(1, 22);
                int hairsColor = Misc.RandomInt(1, 63);
                int hairsColor2 = Misc.RandomInt(1, 63);

                player.SetClothes(2, (ushort)hairs, 0, 0);
                player.HairColor = (byte)hairsColor;
                player.HairHighlightColor = (byte)hairsColor2;
                player.SetEyeColor(3);

                float[] featureParams = { 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1 };
                for (int i = 0; i < featureParams.Length; i++)
                {
                    player.SetFaceFeature((byte)i, featureParams[i]);
                }
            }
        }
        
        public static async Task RefreshClothes(IAltPlayer player)
        {
            if (!Misc.IsResourceLoaded("c_clothesfit"))
                return;

            if (player.Sex == 2)
                return;

            await ClothesFitService.DestroyPlayer(player);
            await ClothesFitService.InitPlayer(player);

            ulong[] outfits = await ClothesFitService.GetOutfitsBySex(player.Sex);

            int index = Misc.RandomInt(0, outfits.Length - 1);
            ulong randomElement = outfits[index];

            await ClothesFitService.Equip(player, (uint)randomElement);
        }

        public static async Task EquipOutfit(IAltPlayer player, uint outfitHash)
        {
            if (!Misc.IsResourceLoaded("c_clothesfit"))
                return;

            if (player.Sex == 2)
                return;

            await ClothesFitService.DestroyPlayer(player);
            await ClothesFitService.InitPlayer(player);

            ulong[] outfits = await ClothesFitService.GetOutfitsBySex(player.Sex);

            if (outfits.Contains(outfitHash))
            {
                await ClothesFitService.Equip(player, outfitHash);
            }
        }
}