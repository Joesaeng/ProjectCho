using System;

namespace Define
{
    public enum GameoverType
    {
        Gameover,
        Clear,
    }
    public enum MagicianAnim
    {
        ProjectileAttack,
        AOEAttack,
        StraightProjectileAttack,
        BeamAttack,
    }

    public enum AttackableState
    {
        SearchTarget,
        Attack,
        Idle,
    }

    public enum UISFX
    {

    }

    [Serializable]
    public enum GameLanguage
    {
        English,
        Korean,
    }

    public enum WorldObject
    {
        Unknown,
    }

    public enum Layer
    {

    }

    public enum Scene
    {
        Unknown,
        Login,
        Lobby,
        Defense,
    }

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount,
    }

    public enum UIEvent
    {
        Click,
        Drag,
    }

    public enum MouseEvent
    {
        Press,
        PointerDown,
        PointerUp,
        Click,
    }
}
