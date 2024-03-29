﻿namespace RambleJungle.Base
{
    public enum JungleObjectType
    {
        Rambler,

        // beasts
        DragonflySwarm,
        WildPig,
        Snakes,
        CarnivorousPlant,
        Minotaur,
        Hydra,

        // hidden items
        LostWeapon,
        Elixir,
        Map,
        Radar,
        MagnifyingGlass,
        Talisman,
        Natives,
        Quicksand,
        Trap,
        Treasure,
        EmptyField,

        // visible items
        Camp,
        Tent,
        ForgottenCity,
        DenseJungle
    }

    public enum WeaponType
    {
        Dagger,
        Torch,
        Spear,
        Machete,
        Bow,
        Battleaxe,
        Sword
    }

    [Flags]
    public enum Statuses
    {
        Hidden = 1, // pole zakryte
        Shown = 2, // pole właśnie odwiedzane
        Visible = 4, // pole zawierające obiekt widzialny
        Visited = 8, // pole odwiedzone
        Pointed = 16, // pole wskazane przez mapę, kompas lub lupę
        Marked = 32, // pole odkryte przez talizman lub po zakończeniu gry
        NotVisited = Hidden | Shown | Visible | Pointed | Marked,
        Explored = Visited | Marked | Pointed
    }

    public enum CampBonus
    {
        Strenght,
        Health,
        Adjacency,
        DoubleAttack
    }
}
