using UnityEngine;

namespace EcoSort.Gameplay
{
    public enum WasteCategory
    {
        TutorialCube = -1,
        GreenOrganic = 0,     // Verde: Orgánico (plátano, manzana, restos de comida)
        GreyGeneral = 1,      // Gris: Bote de Basura Normal (todo menos líquido)
        RedSharps = 2,        // Rojo: Punzocortantes y Agujas (jeringas, agujas)
        BlueInorganic = 3,    // Azul: Inorgánico Reciclable (plástico, latas, cartón)
        YellowGlass = 4,      // Amarillo: Vidrios (botellas y frascos de vidrio)
        OrangeSpecial = 5     // Naranja: Especiales / Pilas y No Reciclables Especiales
    }

    public static class WasteCategoryInfo
    {
        public static string GetBinNameEs(WasteCategory cat)
        {
            return cat switch
            {
                WasteCategory.TutorialCube => "Zona de Entrega",
                WasteCategory.GreenOrganic => "Bote Verde (Orgánico)",
                WasteCategory.GreyGeneral => "Bote Gris (Basura Normal)",
                WasteCategory.RedSharps => "Bote Rojo (Punzocortantes)",
                WasteCategory.BlueInorganic => "Bote Azul (Inorgánico)",
                WasteCategory.YellowGlass => "Bote Amarillo (Vidrios)",
                WasteCategory.OrangeSpecial => "Bote Naranja (Especiales)",
                _ => "Bote"
            };
        }

        public static string GetBinNameEn(WasteCategory cat)
        {
            return cat switch
            {
                WasteCategory.TutorialCube => "Drop Zone",
                WasteCategory.GreenOrganic => "Green Bin (Organic)",
                WasteCategory.GreyGeneral => "Grey Bin (General Waste)",
                WasteCategory.RedSharps => "Red Bin (Sharps & Needles)",
                WasteCategory.BlueInorganic => "Blue Bin (Inorganic)",
                WasteCategory.YellowGlass => "Yellow Bin (Glass)",
                WasteCategory.OrangeSpecial => "Orange Bin (Special Waste)",
                _ => "Bin"
            };
        }

        public static Color GetCategoryColor(WasteCategory cat)
        {
            return cat switch
            {
                WasteCategory.TutorialCube => new Color(0.11f, 0.60f, 0.97f),
                WasteCategory.GreenOrganic => new Color(0.18f, 0.64f, 0.36f),
                WasteCategory.GreyGeneral => new Color(0.44f, 0.50f, 0.56f),
                WasteCategory.RedSharps => new Color(0.78f, 0.18f, 0.18f),
                WasteCategory.BlueInorganic => new Color(0.16f, 0.45f, 0.84f),
                WasteCategory.YellowGlass => new Color(0.90f, 0.71f, 0.07f),
                WasteCategory.OrangeSpecial => new Color(0.90f, 0.44f, 0.00f),
                _ => Color.white
            };
        }
    }
}
