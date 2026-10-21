using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

public interface IDialogueProvider
{
    UniTask<IReadOnlyList<CharacterLineClass>> LoadAsync(object owner, CancellationToken token);
}
