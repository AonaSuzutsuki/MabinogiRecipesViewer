using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookInformationViewer.Models.Db.Context
{
    [Table(Constants.CookEffectsTableName, Schema = Constants.Schema)]
    public class DbCookEffects : DbBase
    {
        [Column("id")]
        public override int Id { get; set; }

        [Column("recipe_id")]
        public int RecipeId { get; set; }

        [Column("number")]
        public int Number { get; set; }

        [Column("star")]
        public int Star { get; set; }

        [Column("ignore_check")]
        public bool IgnoreCheck { get; set; }

        [Column("hp")]
        public int Hp { get; set; }
        [Column("mana")]
        public int Mana { get; set; }
        [Column("stamina")]
        public int Stamina { get; set; }
        [Column("str")]
        public int Str { get; set; }
        [Column("inteli")]
        public int Int { get; set; }
        [Column("dex")]
        public int Dex { get; set; }
        [Column("will")]
        public int Will { get; set; }
        [Column("luck")]
        public int Luck { get; set; }
        [Column("damage")]
        public int Damage { get; set; }
        [Column("magic_damage")]
        public int MagicDamage { get; set; }
        [Column("min_damage")]
        public int MinDamage { get; set; }
        [Column("protection")]
        public int Protection { get; set; }
        [Column("defense")]
        public int Defense { get; set; }
        [Column("magic_protection")]
        public int MagicProtection { get; set; }
        [Column("magic_defense")]
        public int MagicDefense { get; set; }

        [Column("create_date")]
        public override DateTime? CreateDate { get; set; }

        [Column("update_date")]
        public override DateTime? UpdateDate { get; set; }

        [Column("is_delete")]
        public override bool IsDelete { get; set; }

        public override void Apply(object source)
        {
            base.Apply(source);

            if (!(source is DbCookEffects effects))
                return;

            RecipeId = effects.RecipeId;
            Number = effects.Number;
            Star = effects.Star;
            IgnoreCheck = effects.IgnoreCheck;
            Hp = effects.Hp;
            Mana = effects.Mana;
            Stamina = effects.Stamina;
            Str = effects.Str;
            Int = effects.Int;
            Dex = effects.Dex;
            Will = effects.Will;
            Luck = effects.Luck;
            Damage = effects.Damage;
            MagicDamage = effects.MagicDamage;
            MinDamage = effects.MinDamage;
            Protection = effects.Protection;
            Defense = effects.Defense;
            MagicProtection = effects.MagicProtection;
            MagicDefense = effects.MagicDefense;
        }
    }
}