// DoorQuestion.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Door Question", menuName = "Ghost Train/Door Question")]
public class DoorQuestion : ScriptableObject
{
    [TextArea(3, 5)]
    public string question;
    public int correctAnswer;
}