﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    public class Enemy : Damageable
    {

        #region Fields

        private const int statusdefault = 100;
        private byte _fixedLevel;

        #endregion Fields

        #region Constructors

        public Enemy(Module_battle_debug.EnemyInstanceInformation eII, byte fixedLevel = 0)
        {
            EII = eII;
            FixedLevel = fixedLevel;
            _CurrentHP = MaxHP();
            if ((info.bitSwitch & Debug_battleDat.Information.Flag1.Zombie) != 0)
            {
                Statuses0 |= Kernel_bin.Persistant_Statuses.Zombie;
            }
            if ((info.bitSwitch & Debug_battleDat.Information.Flag1.Auto_Protect) != 0)
            {
                Statuses1 |= Kernel_bin.Battle_Only_Statuses.Protect;
            }
            if ((info.bitSwitch & Debug_battleDat.Information.Flag1.Auto_Reflect) != 0)
            {
                Statuses1 |= Kernel_bin.Battle_Only_Statuses.Reflect;
            }
            if ((info.bitSwitch & Debug_battleDat.Information.Flag1.Auto_Shell) != 0)
            {
                Statuses1 |= Kernel_bin.Battle_Only_Statuses.Shell;
            }
            if ((info.bitSwitch & Debug_battleDat.Information.Flag1.Fly) != 0)
            {
                Statuses1 |= Kernel_bin.Battle_Only_Statuses.Float;
            }
        }

        #endregion Constructors

        #region Properties

        public static List<Enemy> Party { get; set; }

        public byte AP => info.ap;
        public Cards.ID Card => info.card[levelgroup()];

        public Debug_battleDat.Magic[] DrawList => hml(info.drawhigh, info.drawmed, info.drawlow);

        /// <summary>
        /// Randomly gain 1 or 0 from this list.
        /// </summary>
        public Saves.Item[] DropList => hml(info.drophigh, info.dropmed, info.droplow);

        public byte DropRate => (byte)(MathHelper.Clamp(info.dropRate * 100 / byte.MaxValue, 0, 100));

        public Module_battle_debug.EnemyInstanceInformation EII { get; set; }

        public byte EVA => convert2(info.eva);

        /// <summary>
        /// The EXP everyone gets.
        /// </summary>
        public int EXP => convert3(info.exp, Memory.State.AveragePartyLevel);

        public byte FixedLevel { get => _fixedLevel; set => _fixedLevel = value; }

        /// <summary>
        /// Level of enemy based on average of party or fixed value.
        /// </summary>
        /// <see cref="https://finalfantasy.fandom.com/wiki/Level#Enemy_levels"/>
        public byte Level
        {
            get
            {
                if (FixedLevel != default)
                    return FixedLevel;
                byte a = Memory.State.AveragePartyLevel;
                byte d = (byte)(a / 5);
                return (byte)MathHelper.Clamp(a + d, 1, 100);
            }
        }

        public byte MAG => convert1(info.mag);

        /// <summary>
        /// Randomly gain 1 or 0 from this list.
        /// </summary>
        public Saves.Item[] MugList => hml(info.mughigh, info.mugmed, info.muglow);

        public byte MugRate => (byte)(MathHelper.Clamp(info.mugRate * 100 / byte.MaxValue, 0, 100));

        public override FF8String Name => info.name;

        public byte SPD => convert2(info.spd);

        public byte SPR => convert2(info.spr);

        public byte STR => convert1(info.str);

        public byte VIT => convert2(info.vit);

        public Kernel_bin.Devour Devour => info.devour[levelgroup()] >= Kernel_bin.Devour_.Count ?
            Kernel_bin.Devour_[Kernel_bin.Devour_.Count - 1] :
            Kernel_bin.Devour_[info.devour[levelgroup()]];

        private Debug_battleDat.Information info => EII.Data.information;

        #endregion Properties

        #region Methods

        public static implicit operator Enemy(Module_battle_debug.EnemyInstanceInformation @in) => new Enemy(@in);

        public Saves.Item Drop() => chance(DropRate, DropList);

        public short ElementalResistance(Kernel_bin.Element @in)
        {
            List<Kernel_bin.Element> l = (Enum.GetValues(typeof(Kernel_bin.Element))).Cast<Kernel_bin.Element>().ToList();
            if (@in == Kernel_bin.Element.Non_Elemental)
                return 100;
            // I wonder if i should average the resistances in cases of multiple elements.
            else
                return conv(info.resistance[l.FindIndex(x => (x & @in) != 0) - 1]);
            short conv(byte val) => (short)MathHelper.Clamp(900 - (val * 10), -100, 400);
        }

        /// <summary>
        /// The character whom lands the last hit gets alittle bonus xp.
        /// </summary>
        /// <param name="lasthitlevel">Level of character whom got last hit.</param>
        /// <returns></returns>
        public int EXPExtra(byte lasthitlevel) => convert3(info.expExtra, lasthitlevel);

        public override ushort MaxHP()
        {
            //from Ifrit's help file
            int i = (info.hp[0] * Level * Level / 20) + (info.hp[0] + info.hp[2] * 100) * Level + info.hp[1] * 10 + info.hp[3] * 1000;
            return (ushort)MathHelper.Clamp(i, 0, ushort.MaxValue);
        }

        public Saves.Item Mug() => chance(MugRate, MugList);

        /// <summary>
        /// I notice that the resistance reported on the wiki is 100 less than the number in the data.
        /// </summary>
        /// <param name="s">status effect</param>
        /// <returns>percent of resistance</returns>
        /// <see cref="https://finalfantasy.fandom.com/wiki/G-Soldier#Stats"/>
        public sbyte StatusResistance(Kernel_bin.Persistant_Statuses s)
        {
            byte r = 100;
            switch (s)
            {
                case Kernel_bin.Persistant_Statuses.Death:
                    r = info.deathResistanceMental;
                    break;

                case Kernel_bin.Persistant_Statuses.Poison:
                    r = info.poisonResistanceMental;
                    break;

                case Kernel_bin.Persistant_Statuses.Petrify:
                    r = info.petrifyResistanceMental;
                    break;

                case Kernel_bin.Persistant_Statuses.Darkness:
                    r = info.darknessResistanceMental;
                    break;

                case Kernel_bin.Persistant_Statuses.Silence:
                    r = info.silenceResistanceMental;
                    break;

                case Kernel_bin.Persistant_Statuses.Berserk:
                    r = info.berserkResistanceMental;
                    break;

                case Kernel_bin.Persistant_Statuses.Zombie:
                    r = info.zombieResistanceMental;
                    break;
            }

            return (sbyte)MathHelper.Clamp(r - 100, -100, 100);
        }

        /// <summary>
        /// I notice that the resistance reported on the wiki is 100 less than the number in the data.
        /// </summary>
        /// <param name="s">status effect</param>
        /// <returns>percent of resistance</returns>
        /// <see cref="https://finalfantasy.fandom.com/wiki/G-Soldier#Stats"/>
        public sbyte StatusResistance(Kernel_bin.Battle_Only_Statuses s)
        {
            byte r = statusdefault;
            switch (s)
            {
                case Kernel_bin.Battle_Only_Statuses.Sleep:
                    r = info.sleepResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Haste:
                    r = info.hasteResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Slow:
                    r = info.slowResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Stop:
                    r = info.stopResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Regen:
                    r = info.regenResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Protect:
                    break;

                case Kernel_bin.Battle_Only_Statuses.Shell:
                    break;

                case Kernel_bin.Battle_Only_Statuses.Reflect:
                    r = info.reflectResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Aura:
                    break;

                case Kernel_bin.Battle_Only_Statuses.Curse:
                    break;

                case Kernel_bin.Battle_Only_Statuses.Doom:
                    r = info.doomResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Invincible:
                    break;

                case Kernel_bin.Battle_Only_Statuses.Petrifying:
                    r = info.slowPetrifyResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Float:
                    r = info.floatResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Confuse:
                    r = info.confuseResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Drain:
                    r = info.drainResistanceMental;
                    break;

                case Kernel_bin.Battle_Only_Statuses.Eject:
                    r = info.explusionResistanceMental;
                    break;
            }
            return (sbyte)MathHelper.Clamp(r - 100, -100, 100);
        }

        /// <summary>
        /// The wiki says some areas have forced or random levels. This lets you override the level.
        /// </summary>
        /// <see cref="https://finalfantasy.fandom.com/wiki/Level#Enemy_levels"/>
        public override string ToString() => Name.Value_str;

        public static implicit operator Module_battle_debug.EnemyInstanceInformation(Enemy @in) => @in.EII;

        private T chance<T>(byte percent, T[] list)
        {
            int i = Memory.Random.Next(0, 100);
            if (i < percent && list.Length > 0)
            {
                i = Memory.Random.Next(0, list.Length-1);
                return list[i];
            }
            return default;
        }

        private byte convert1(byte[] @in)
        {
            //from Ifrit's help file
            byte level = Level;
            int i = level * @in[0] / 10 + level / @in[1] - level * level / 2 / (@in[3] + @in[2]) / 4;
            //PLEASE NOTE: I'm not 100% sure on the STR/MAG formula, but it should be accurate enough to get the general idea.
            // wiki states something like ([3(Lv)] + [(Lv) / 5] - [(Lv)² / 260] + 12) / 4

            return (byte)MathHelper.Clamp(i, 0, byte.MaxValue);
        }

        private byte convert2(byte[] @in)
        {
            //from Ifrit's help file
            byte level = Level;
            int i = level / @in[1] - level / @in[3] + level * @in[0] + @in[2];
            return (byte)MathHelper.Clamp(i, 0, byte.MaxValue);
        }

        private int convert3(ushort @in, byte inLevel)
        {
            //from Ifrit's help file
            byte level = Level;
            return @in * (5 * (level - inLevel) / inLevel + 4);
        }

        private T hml<T>(T h, T m, T l)
        {
            byte level = Level;
            if (level > info.highLevelStart)
                return h;
            else if (level > info.medLevelStart)
                return m;
            else return l;
        }
        private int levelgroup()
        {
            byte l = Level;
            if (l > info.highLevelStart)
                return 2;
            if (l > info.medLevelStart)
                return 1;
            else return 0;
        }

        #endregion Methods

    }
}