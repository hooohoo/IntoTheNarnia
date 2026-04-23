# UI 시스템

> 예시의 `{Feature}`는 실제 기능명으로 치환. 실제 네이밍은 `/feature`·`/ui` 커맨드의 "최근 참고 코드 자동 탐색" 결과 사용. `Assets/@Project/01.Scripts/UI/`, `Assets/@Project/01.Scripts/Manager/Manager/UIManager.cs`

## 1. 상속 구조

```
UI_Base : MonoBehaviour <<abstract>>
├── UIKey, SortOrder
├── abstract Init() / abstract Release()
├── virtual Open() / virtual Close()
│
├── UI_Scene — Release() → CloseSubUIs(this)
├── UI_Popup — Release() → CloseSubUIs(this)
└── UI_Sub   — OwnerUI, OnSpawn(), OnDespawn()
```

## 2. UIManager 3계층

| 레이어 | 자료구조 | SortOrder | API |
|--------|---------|-----------|-----|
| **Scene** | `Dict<string, UI_Scene>` 캐싱 | 0~99 (+1) | `ShowSceneUI<T>(key)` / `CloseSceneUI(key)` |
| **Popup** | `Dict` + `Stack`(LIFO) + `HashSet` | 100+ (+10) | `ShowPopupUI<T>(key)` / `CloseTopPopup()` |
| **Sub** | `Dict<string, Queue<UI_Sub>>` 풀링 | 부모 따름 | `ShowSubUI<T>(key, parent)` / `CloseSubUI()` |

- Scene/Popup: 첫 생성 시 Addressable 로드 → Init → 캐시. 재사용 시 Open.
- Sub: 풀에 있으면 Dequeue, 없으면 Instantiate. OnSpawn → 사용 → OnDespawn → Enqueue.
- 부모 Release 시 `CloseSubUIs(owner)`로 자식 Sub 일괄 정리.

## 3. CanvasScaler 관리

`UIManager.Layout.cs`가 `IListener<LayoutEvent>` 구현. `LayoutEvent.ScalerUpdate` 수신 → 모든 CanvasScaler `referenceResolution` 일괄 변경.

## 4. 풀링 수명주기

| 시스템 | 생성 | 반환 | 콜백 |
|--------|------|------|------|
| **ObjectManager** | `Spawn<T>(key)` | `Despawn(obj)` → Queue | Init/Release |
| **UIManager Sub** | `ShowSubUI<T>()` | `CloseSubUI()` → Queue | OnSpawn/OnDespawn |
| **SoundManager SFX** | Pool 생성 | Complete → Available | - |

## 5. UI 아키텍처 패턴

### 기본값 — MVC 분리

모든 UI_Scene / UI_Popup / UI_Sub는 기본적으로 Controller + Model + View (+ Service) 분리. 정렬/필터링 등 로직이 커지면 Service 추가.

### 예외 — Controller 단일

다음 조건을 **모두** 만족할 때만 허용:
- 외부 데이터(API / EventManager / DataManager)에 의한 상태 없음
- UI 요소 간 얽힌 상태 변화 없음
- 입력 → 반영 1단 경로

예: 뒤로가기 버튼, 정적 배경, 단순 탭. 애매하면 MVC.

### MVC 상세

```
External ──▶ Controller
              │ 명령
       ┌──────┼──────┐
       ▼      ▼      ▼
     Model  View  Service
       │      │
       └─구독─┘  (delegate)
```

| 역할 | 책임 | Model 참조 |
|------|------|-----------|
| **Controller** | 외부 호출 창구, Model/View 생성, `view.Bind(model)`, 명령 전달 | O |
| **Model** | 데이터 상태 + 비즈니스 로직, delegate로 변경 알림 | - |
| **View** | UI 렌더링, `Bind(model)`로 Model을 **직접 구독** | O |
| **Service** | Controller 비대화 시 추가 (정렬·필터링 등) | 상황에 따라 |

**핵심: View가 `Bind(model)`로 Model delegate를 스스로 구독. Controller는 연결하지 않음.**

```csharp
// Controller (UI_Scene)
public class UI_{Feature}ItemList : UI_Scene, IListener<{Feature}Event>
{
    private {Feature}Model model;
    private {Feature}View view;

    public override void Init()
    {
        model = new {Feature}Model();
        view = new {Feature}View();
        view.Initialize(contentRect);
        view.Bind(model);                // View가 Model 직접 구독

        model.OnItemApplied += ApplyItem;
        eventManager.AddListener(this);
    }

    public void OnEvent({Feature}Event e, object p = null)
    {
        if (e == {Feature}Event.ItemSelected)
            model.ToggleEquip(((UI_{Feature}ItemSlot)p).ItemData);
    }

    public override void Release()
    {
        view.Unbind();                   // View가 스스로 해제
        model.OnItemApplied -= ApplyItem;
        view.Release();
        eventManager.RemoveListener(this);
        base.Release();
    }
}

// Model — View/Controller를 모름. delegate로만 알림
public class {Feature}Model
{
    public event Action<Item[], HashSet<string>> OnPageChanged;
    public event Action<string, bool> OnItemApplied;
    public void ToggleEquip(Item item) { /* ... */ OnItemApplied?.Invoke(item.Id, equipped); }
}

// View — Bind로 Model 직접 구독
public class {Feature}View
{
    private {Feature}Model model;
    public void Initialize(RectTransform content) { /* contentRect 저장 */ }
    public void Bind({Feature}Model m)   { model = m; m.OnPageChanged += SetData; }
    public void Unbind()                 { if (model == null) return; model.OnPageChanged -= SetData; model = null; }
    private void SetData(Item[] items, HashSet<string> equipped) { /* 슬롯 갱신 */ }
    public void Release() { Unbind(); }
}
```

### Sub UI MVC (슬롯 풀링)

```csharp
[RequireComponent(typeof(SlotModel), typeof(SlotView))]
public class UI_{Feature}ItemSlot : UI_Sub
{
    private SlotModel model;
    private SlotView view;

    public override void Init()
    {
        model = GetComponent<SlotModel>();
        view  = GetComponent<SlotView>();
        view.SetModel(model);
    }
    public override void OnSpawn()   { view.SubscribeModel(); }                 // View가 구독
    public override void OnDespawn() { view.UnsubscribeModel(); model.Reset(); } // View가 해제

    public void SetData(Item item, bool equipped) { model.SetData(item, equipped); }
}
```

## 6. 구현 가이드 — 새 UI 추가

### UI_Scene 시그니처 골격

```csharp
public class UI_{Feature} : UI_Scene, IInputEventReceiver, IListener<{Feature}Event>
{
    [Singleton(typeof(InputManager))] private InputManager inputManager;
    [Singleton(typeof(EventManager))] private EventManager eventManager;
    [FindComponents("Content"), SerializeField] private RectTransform content;

    public override void Init()
    {
        InjectUtil.InjectComponents(this);    // FindComponents 바인딩
        InjectUtil.InjectSingleton(this);
        inputManager.RegisterReceiver(this);
        eventManager.AddListener(this);
    }

    public void OnEvent({Feature}Event e, object p = null) { /* switch-case */ }
    public void OnInputEvent(InputEvent e, InputEventData d) { if (e == InputEvent.Tap) /* ... */ }
    public void OnPinchEvent(InputEvent e, PinchData d) { }

    public override void Release()
    {
        inputManager.UnregisterReceiver(this);
        eventManager.RemoveListener(this);
        base.Release();                       // → CloseSubUIs(this)
    }
}
```

### UI_Sub 시그니처 골격 (풀링)

```csharp
public class UI_{Feature}ItemSlot : UI_Sub, IInputEventReceiver
{
    [Singleton(typeof(InputManager))] private InputManager inputManager;
    [FindComponents("Icon"), SerializeField] private Image icon;

    public override void Init()      { InjectUtil.InjectComponents(this); InjectUtil.InjectSingleton(this); }
    public override void OnSpawn()   { inputManager.RegisterReceiver(this); }   // 풀에서 꺼낼 때
    public override void OnDespawn() { inputManager.UnregisterReceiver(this); } // 풀에 반환할 때

    public void OnInputEvent(InputEvent e, InputEventData d)
    {
        if (e == InputEvent.Tap && d.hitObject == icon.gameObject)
            eventManager.PostNotification({Feature}Event.ItemSelected, this);
    }
    public void OnPinchEvent(InputEvent e, PinchData d) { }
    public override void Release() { inputManager.UnregisterReceiver(this); }
}
```

### 추가 절차

1. `UIKeys` 중첩 클래스에 키 추가 (Scene/Popup/Sub 3개 중첩 static class로 구분)
   ```csharp
   public static class UIKeys
   {
       public static class Scene  { public const string UI_{Feature} = "UI_{Feature}"; }
       public static class Popup  { public const string UI_{Feature}Detail = "UI_{Feature}Detail"; }
       public static class Sub    { public const string UI_{Feature}ItemSlot = "UI_{Feature}ItemSlot"; }
   }
   ```
2. 프리팹 생성 (이름 = UIKeys 값, 루트에 **BindRoot 태그**)
3. Addressable에 프리팹 등록 (라벨: `UI`)
4. 사용: `uiManager.ShowSceneUI<...>(UIKeys.Scene.UI_{Feature})` / `ShowPopupUI<...>` / `ShowSubUI<...>(key, parent)`

### 체크리스트

**UI 고유**
- [ ] UI_Scene / UI_Popup / UI_Sub 중 선택하여 상속
- [ ] UIKeys 중첩 클래스(Scene/Popup/Sub)에 키 추가
- [ ] Addressable에 프리팹 등록 (라벨: `UI`), 프리팹 루트 `BindRoot` 태그
- [ ] Init: `InjectUtil.InjectComponents(this)` + `InjectUtil.InjectSingleton(this)`
- [ ] `[FindComponents("자식이름")]`이 프리팹 자식명과 일치
- [ ] 입력 필요 시 `IInputEventReceiver` + Register/Unregister
- [ ] 이벤트 필요 시 `IListener<T>` + AddListener/RemoveListener
- [ ] **기본 MVC**: `view.Bind(model)` / Release에서 `view.Unbind()`
- [ ] 단일 Controller 예외 적용 시 "외부 데이터·복잡 상태 없음"을 주석으로 명시
- [ ] Sub UI MVC: OnSpawn에서 `view.SubscribeModel()`, OnDespawn에서 `view.UnsubscribeModel()` + `model.Reset()`
- [ ] Release에서 모든 구독/등록 해제 (EventManager, InputManager, delegate), DOTween 사용 시 `DOKill()`

**컨벤션 핵심** (상세는 `Roles/Convention/`)
- [ ] `private static readonly string LogTag = "[클래스명]";` + `LogUtil` 로깅
- [ ] 비동기 메서드 `Async` 접미어 + Fire-and-Forget `.Forget()`
- [ ] `Update/FixedUpdate` 금지 — UniTask 또는 `Initialize()`
- [ ] LINQ / 람다 금지 (외부 라이브러리 제외)
- [ ] `Camera.main` 금지 → `cameraManager.MainCamera`
