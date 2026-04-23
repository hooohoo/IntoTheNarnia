# Manager 컨벤션

적용 대상: `Assets/@Project/01.Scripts/Manager/Manager/**/*.cs` 경로의 매니저 클래스.

## 체크리스트

- [ ] Partial 분할 파일명이 `ClassName.DomainName.cs` 이고 base 파일에 `partial void InitXxx()` / `partial void ReleaseXxx()` 선언?

---

## 상세 규칙

### Partial Class 분할

매니저가 커지면 도메인 단위로 partial class 분할. 파일 하나가 한 책임 영역을 담당한다.

- 파일명: `ClassName.DomainName.cs` (예: `DataManager.KnowledgeGraph.cs`, `APIManager.Request.cs`)
- base 파일(`ClassName.cs`)에 `partial void InitXxx()`, `partial void ReleaseXxx()` 선언
- 각 도메인 파일에서 해당 partial 메서드 구현
- 도메인 간 공유 상태(예: 주입 필드)는 base 파일에 둔다

### 예시 — DataManager

```csharp
// DataManager.cs (base)
public partial class DataManager : Singleton<DataManager>, IManager
{
    private static readonly string LogTag = "[DataManager]";

    [Singleton(typeof(AddressableManager))]
    private AddressableManager addressableManager;

    public void Init()
    {
        InjectUtil.InjectSingleton(this);
        InitKnowledgeGraph();
        InitGNB();
        // ...
    }

    public void Release()
    {
        ReleaseKnowledgeGraph();
        ReleaseGNB();
        // ...
    }

    partial void InitKnowledgeGraph();
    partial void ReleaseKnowledgeGraph();
    partial void InitGNB();
    partial void ReleaseGNB();
}

// DataManager.KnowledgeGraph.cs
public partial class DataManager
{
    private IKnowledgeGraphProvider knowledgeGraphProvider;

    partial void InitKnowledgeGraph()
    {
        knowledgeGraphProvider = new JsonKnowledgeGraphProvider(
            JsonDataKeys.GalaxyStaticData,
            JsonDataKeys.NodeStaticData,
            JsonDataKeys.RoadMapMetadata
        );
    }

    partial void ReleaseKnowledgeGraph()
    {
        knowledgeGraphProvider = null;
    }

    public void SetKnowledgeGraphProvider(IKnowledgeGraphProvider provider)
    {
        knowledgeGraphProvider = provider;
    }
}
```

### 주의

- base 파일에 `partial void InitXxx()` / `partial void ReleaseXxx()` **선언만** 있고 도메인 파일에 구현이 없으면 컴파일러가 해당 호출을 제거. 의도한 바일 수 있으나, 실수 방지를 위해 선언/구현/호출 3자를 함께 유지할 것.
- Init/Release 호출 등록을 base 파일의 `Init()` / `Release()` 에 반드시 추가. `partial void` 선언만 있고 호출이 빠지면 도메인 초기화가 누락된다.
- 도메인 파일 간 직접 호출 지양. 같은 클래스의 private 메서드지만 의존성이 암묵적으로 생겨 분할 의미가 흐려진다.
