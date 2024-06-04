using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CookInformationViewer.Models.Db.Context;

namespace CookInformationViewer.Models.DataValue
{
    public class EffectInfo
    {
        public int RecipeId { get; set; }
        
        public int Number { get; set; }
        
        public int Star { get; set; }
        
        public int Hp { get; set; }
        public int Mana { get; set; }
        public int Stamina { get; set; }
        public int Str { get; set; }
        public int Int { get; set; }
        public int Dex { get; set; }
        public int Will { get; set; }
        public int Luck { get; set; }
        public int Damage { get; set; }
        public int MagicDamage { get; set; }
        public int MinDamage { get; set; }
        public int Protection { get; set; }
        public int Defense { get; set; }
        public int MagicProtection { get; set; }
        public int MagicDefense { get; set; }

        public EffectInfo(DbCookEffects effect)
        {
            RecipeId = effect.RecipeId;
            Number = effect.Number;
            Star = effect.Star;
            Hp = effect.Hp;
            Mana = effect.Mana;
            Stamina = effect.Stamina;
            Str = effect.Str;
            Int = effect.Int;
            Dex = effect.Dex;
            Will = effect.Will;
            Luck = effect.Luck;
            MinDamage = effect.MinDamage;
            Damage = effect.Damage;
            MagicDamage = effect.MagicDamage;
            Defense = effect.Defense;
            Protection = effect.Protection;
            MagicDefense = effect.MagicDefense;
            MagicProtection = effect.MagicProtection;
        }

        public EffectInfo()
        {

        }
    }
}
