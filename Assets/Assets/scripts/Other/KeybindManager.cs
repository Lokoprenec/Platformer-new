using UnityEngine;

public class KeybindManager : MonoBehaviour
{
    public KeyCode Select;
    public KeyCode Exit;
    public KeyCode Up;
    public KeyCode Down;
    public KeyCode Left;
    public KeyCode Right;
    public KeyCode Jump;
    public KeyCode Attack;
    public KeyCode Heal;
    public KeyCode Bash;
}

public enum Controls
{
    Select,
    Exit,
    Up,
    Down,
    Left,
    Right,
    Jump,
    Attack,
    Heal,
    Bash
}