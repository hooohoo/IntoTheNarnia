# 데이터 레이어

> 예시의 `{Domain}`은 실제 도메인명으로 치환. 실제 네이밍은 `/feature`·`/data` 커맨드의 "최근 참고 코드 자동 탐색" 결과 사용. `Assets/@Project/01.Scripts/Manager/Manager/DataManager/`, `Assets/@Project/01.Scripts/Data/`

## 1. DataManager (partial 7파일)

```
DataManager : Singleton<DataManager>, IManager
├── [Singleton] AddressableManager
├── .cs               — Init/Release, partial 메서드 선언
├── .Avatar           — 아바타 장비/지갑/상점
├── .Friend           — 친구 목록
├── .GNB              — GNB 데이터
├── .Notice           — 공지사항
├── .CreateAvatar     — 아바타 생성
└── .StyleRoom        — 스타일룸 아이템
```

각 partial은 **Provider 인터페이스 + Dictionary 캐시**를 보유.

## 2. Provider 패턴

Provider 인터페이스마다 Json/Api 구현체가 짝을 이룸 (예: `IAvatarDataProvider` → `JsonAvatarDataProvider` / `ApiAvatarDataProvider`).

런타임 교체: `SetProvider(provider) → ClearCache()` (캐시 자동 무효화).

## 3. 캐시 구조

```csharp
Dictionary<long, {Domain}Data> {domain}Cache  // userId별 분리
```

패턴: 캐시 확인 → 있으면 즉시 반환 → 없으면 Provider 호출 → 캐시 저장.

## 4. APIManager (partial 4파일)

```
APIManager : Singleton<APIManager>, IManager
├── .cs       — baseUrl, defaultHeaders
├── .Request  — Get/Post/Put/Delete → IRequestHandler 위임
├── .Response — JSON 파싱, NotifyApiError → ErrorEventManager
└── .Token    — ITokenProvider (Flutter/Web/Editor)
```

## 5. 씬 초기화

- **GameSceneManager**: `Stack<SceneType>` 히스토리, `TransitionToSceneAsync`(FadeOut → 씬 로드 → 리소스 정리 → 메모리 GC → FadeIn).
- **Main.cs**: `Start() → InitializeAsync().Forget()` → Addressable 프리로드 → State 등록 → Screen 초기화 → `ChangeState()` → `SceneEvent.SceneReady`.
- **IScreenInitializer**: `Initialize()` / `Release()`. 구현체: LobbyScreen, StyleRoomScreen 등.

## 6. 구현 가이드 — 새 데이터 도메인 추가

### Step 1: Provider 인터페이스

```csharp
// Assets/@Project/01.Scripts/Data/Provider/{Domain}/I{Domain}DataProvider.cs
public interface I{Domain}DataProvider
{
    UniTask<List<{Domain}Item>> GetItemsAsync(long userId);
    UniTask<bool> SaveItemAsync(long userId, {Domain}Item item);
}
```

### Step 2: 구현체

`Json{Domain}DataProvider` (Addressable JSON 로드) 또는 `Api{Domain}DataProvider` (APIManager 위임). 시그니처는 Step 1 인터페이스와 동일, 각자의 리소스 소스만 다름.

### Step 3: `DataManager.cs` 본체에 등록

```csharp
// 클래스 본체
partial void Init{Domain}();
partial void Release{Domain}();

// Init() 내부
Init{Domain}();

// Release() 내부
Release{Domain}();
```

선언만 하면 초기화 안 됨 — 반드시 Init()/Release() 안에 호출 추가.

### Step 4: `DataManager.{Domain}.cs` partial

```csharp
public partial class DataManager
{
    private I{Domain}DataProvider {domain}Provider;
    private Dictionary<long, List<{Domain}Item>> {domain}Cache = new();

    partial void Init{Domain}()
    {
        {domain}Provider = new Api{Domain}DataProvider();
    }

    public async UniTask<List<{Domain}Item>> Get{Domain}ItemsAsync(long userId)
    {
        if ({domain}Cache.TryGetValue(userId, out var cached))
            return cached;

        var items = await {domain}Provider.GetItemsAsync(userId);
        {domain}Cache[userId] = items;
        return items;
    }

    public void Clear{Domain}Cache() { {domain}Cache.Clear(); }

    partial void Release{Domain}()
    {
        Clear{Domain}Cache();
        {domain}Provider = null;
    }
}
```

### 체크리스트

**Data 고유**
- [ ] Provider 인터페이스 정의 (메서드에 `Async` 접미어 + UniTask)
- [ ] Json/Api 구현체 작성
- [ ] `DataManager.cs` 본체: `partial void InitXxx()` / `partial void ReleaseXxx()` 선언 + Init()/Release() 내 호출 등록
- [ ] `DataManager.Xxx.cs` partial: Provider 필드 + 캐시 + Init/조회/Release
- [ ] 캐시 패턴: 확인 → 있으면 반환 → 없으면 Provider 호출 → 저장
- [ ] ReleaseXxx: 캐시 Clear + Provider null

**컨벤션 핵심** (상세는 `Roles/Convention/`)
- [ ] `private static readonly string LogTag = "[클래스명]";` + `LogUtil` 로깅
- [ ] 비동기 메서드 `Async` 접미어 + Fire-and-Forget `.Forget()`
- [ ] `Update/FixedUpdate` 금지 — UniTask 또는 `Initialize()`
- [ ] LINQ / 람다 금지 (외부 라이브러리 제외)
- [ ] `Camera.main` 금지 → `cameraManager.MainCamera`
