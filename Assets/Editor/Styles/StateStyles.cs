using System.Collections.Generic;
using UnityEngine;

namespace MHFSM
{
    public enum Style { 
        Normal=0,
        Blue,
        Mint,
        Green,
        Yellow,
        Orange,
        Red,
        NormalOn,
        BlueOn,
        MintOn,
        GreenOn,
        YellowOn,
        OrangeOn,
        RedOn,

        NormalHex,
        BlueHex,
        MintHex,
        GreenHex,
        YellowHex,
        OrangeHex,
        RedHex,
        NormalOnHex,
        BlueOnHex,
        MintOnHex,
        GreenOnHex,
        YellowOnHex,
        OrangeOnHex,
        RedOnHex, 
    }

    public class StateStyles
    {
        private static Dictionary<Style, GUIStyle> _styleDictionary = null;
        static StateStyles()
        {

            _styleDictionary = new Dictionary<Style, GUIStyle>();

            _styleDictionary.Add(Style.Normal, new GUIStyle($"flow node {(int)Style.Normal}"));
            _styleDictionary.Add(Style.Blue, new GUIStyle($"flow node {(int)Style.Blue}"));
            _styleDictionary.Add(Style.Mint, new GUIStyle($"flow node {(int)Style.Mint}"));
            _styleDictionary.Add(Style.Green, new GUIStyle($"flow node {(int)Style.Green}"));
            _styleDictionary.Add(Style.Yellow, new GUIStyle($"flow node {(int)Style.Yellow}"));
            _styleDictionary.Add(Style.Orange, new GUIStyle($"flow node {(int)Style.Orange}"));
            _styleDictionary.Add(Style.Red, new GUIStyle($"flow node {(int)Style.Red}"));
            _styleDictionary.Add(Style.NormalOn, new GUIStyle($"flow node {(int)Style.Normal} on"));
            _styleDictionary.Add(Style.BlueOn, new GUIStyle($"flow node {(int)Style.Blue} on"));
            _styleDictionary.Add(Style.MintOn, new GUIStyle($"flow node {(int)Style.Mint} on"));
            _styleDictionary.Add(Style.GreenOn, new GUIStyle($"flow node {(int)Style.Green} on"));
            _styleDictionary.Add(Style.YellowOn, new GUIStyle($"flow node {(int)Style.Yellow} on"));
            _styleDictionary.Add(Style.OrangeOn, new GUIStyle($"flow node {(int)Style.Orange} on"));
            _styleDictionary.Add(Style.RedOn, new GUIStyle($"flow node {(int)Style.Red} on"));


            _styleDictionary.Add(Style.NormalHex, new GUIStyle($"flow node hex {(int)Style.Normal}"));
            _styleDictionary.Add(Style.BlueHex, new GUIStyle($"flow node hex {(int)Style.Blue}"));
            _styleDictionary.Add(Style.MintHex, new GUIStyle($"flow node hex {(int)Style.Mint}"));
            _styleDictionary.Add(Style.GreenHex, new GUIStyle($"flow node hex {(int)Style.Green}"));
            _styleDictionary.Add(Style.YellowHex, new GUIStyle($"flow node hex {(int)Style.Yellow}"));
            _styleDictionary.Add(Style.OrangeHex, new GUIStyle($"flow node hex {(int)Style.Orange}"));
            _styleDictionary.Add(Style.RedHex, new GUIStyle($"flow node hex {(int)Style.Red}"));
            _styleDictionary.Add(Style.NormalOnHex, new GUIStyle($"flow node hex {(int)Style.Normal} on"));
            _styleDictionary.Add(Style.BlueOnHex, new GUIStyle($"flow node hex {(int)Style.Blue} on"));
            _styleDictionary.Add(Style.MintOnHex, new GUIStyle($"flow node hex {(int)Style.Mint} on"));
            _styleDictionary.Add(Style.GreenOnHex, new GUIStyle($"flow node hex {(int)Style.Green} on"));
            _styleDictionary.Add(Style.YellowOnHex, new GUIStyle($"flow node hex {(int)Style.Yellow} on"));
            _styleDictionary.Add(Style.OrangeOnHex, new GUIStyle($"flow node hex {(int)Style.Orange} on"));
            _styleDictionary.Add(Style.RedOnHex, new GUIStyle($"flow node hex {(int)Style.Red} on"));

        }

        public static GUIStyle Get(Style style)
        {
            return _styleDictionary.GetValueOrDefault(style);
        }
    }
}
