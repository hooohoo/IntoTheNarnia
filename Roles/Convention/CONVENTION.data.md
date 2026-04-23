# Data 컨벤션

적용 대상: `Assets/@Project/01.Scripts/Data/Provider/**`, 또는 파일명이 `DataManager.*.cs` / `APIManager.*.cs` 인 partial 파일.

## 체크리스트

- [ ] Provider 구현체 네이밍(`Json*` / `Api*` / `Flutter*` / `Web*` / `Editor*`)?
- [ ] Dictionary 캐시 패턴(`TryGetValue` 확인 → 있으면 반환 → 없으면 Provider 호출 → 저장) 준수?
- [ ] `ReleaseXxx` 에서 캐시 `Clear()` + Provider null 처리?

---

## 상세 규칙

### Provider 패턴 네이밍

외부 의존(API / DB / 파일 / 플랫폼)은 `I<Domain>Provider` 인터페이스로 추상화하고, 구현체는 출처에 따라 prefix 를 통일한다 (공통 컨벤션의 "인터페이스 활용" 의 구체화).

인터페이스: `I<Domain>Provider` / `I<Domain>DataProvider`

예: `IKnowledgeGraphProvider`, `IGNBDataProvider`, `ITokenProvider`, `INoticeDataProvider`.

구현체 prefix:

| prefix | 출처 | 예시 |
|---|---|---|
| `Json*` | JSON 파일 로컬 로드 | `JsonKnowledgeGraphProvider`, `JsonGNBDataProvider` |
| `Api*` | REST API 서버 | `ApiKnowledgeGraphProvider`, `ApiGNBDataProvider` |
| `Flutter*` | Flutter Bridge 경유 | `FlutterTokenProvider` |
| `Web*` | Web Bridge 경유 | `WebTokenProvider` |
| `Editor*` | Unity Editor 전용 | `EditorTokenProvider` |

### 교체 API

Provider 교체는 공개 메서드로 제공한다. 런타임 플랫폼 판별이나 테스트 시 교체용.

```csharp
public void SetKnowledgeGraphProvider(IKnowledgeGraphProvider provider)
{
    knowledgeGraphProvider = provider;
    ClearKnowledgeGraphCache();
}
```

### Dictionary 캐시 패턴

데이터 조회는 Dictionary 기반 캐시를 거쳐 Provider 호출 빈도를 줄인다. 표준 플로우: 캐시 확인 → 있으면 반환 → 없으면 Provider 호출 → 저장 후 반환.

```csharp
public partial class DataManager
{
    private IGNBDataProvider gnbProvider;
    private Dictionary<long, GNBData> gnbCache = new();

    public async UniTask<GNBData> GetGNBDataAsync(long userId)
    {
        if (gnbCache.TryGetValue(userId, out var cached))
        {
            return cached;
        }

        var data = await gnbProvider.GetGNBDataAsync(userId);
        gnbCache[userId] = data;
        return data;
    }
}
```

- 캐시 키는 일반적으로 `userId` 또는 `(userId, category)` 등 조회 범위에 대응.
- 같은 도메인에서 조회용·저장용이 섞이면 저장 직후 해당 키 엔트리를 명시적으로 갱신하거나 제거할 것.

### ReleaseXxx 패턴

partial 도메인 파일의 `ReleaseXxx()` 는 캐시와 Provider 참조를 모두 정리한다.

```csharp
public partial class DataManager
{
    partial void ReleaseGNB()
    {
        ClearGNBCache();
        gnbProvider = null;
    }

    public void ClearGNBCache()
    {
        gnbCache.Clear();
    }
}
```

- 캐시 `Clear()` — 내부 Dictionary 항목을 모두 비움
- Provider null — 주입된 구현체 참조를 끊어 GC 가능 상태로 전환
- Provider 교체(`SetXxxProvider`) 시에도 동일한 캐시 무효화를 해야 스테일 데이터 반환을 방지
