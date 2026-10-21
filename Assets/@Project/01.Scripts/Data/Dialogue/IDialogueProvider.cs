using System.Collections.Generic;

public interface IDialogueProvider
{
    IReadOnlyList<CharacterLineClass> Load(string text);
}
