# UI 컨벤션

적용 대상: `UI_Scene` / `UI_Popup` / `UI_Sub` / `UI_Base` 상속 클래스, 또는 경로 `Assets/@Project/01.Scripts/UI/**`.

## 체크리스트

- [ ] `UI_Scene` / `UI_Popup` / `UI_Sub` 파생 클래스에 `UI_` 접두어? (UI 헬퍼·Controller·Strategy 류는 생략)
- [ ] `UIKeys` 중첩 구조(Scene/Popup/Sub 3개 내부 static class)로 키 분리?
- [ ] UI 3계층 sortOrder 규칙(Scene +1 / Popup 100 부터 +10 Stack / Sub Queue 풀링)?
- [ ] MVC 적용 시 View 가 `Bind(model)` 로 Model 직접 구독 / Release 에서 `Unbind()`?
- [ ] Sub UI 풀링 시 `OnSpawn` 에서 `SubscribeModel`, `OnDespawn` 에서 `UnsubscribeModel` + `model.Reset()`?
- [ ] 단일 Controller 예외 적용 시 3조건(외부 데이터 없음·UI 간 얽힌 상태 없음·입력→반영 1단) 주석 명시?
- [ ] `IInputEventReceiver` 구현 시 `RegisterReceiver` / `UnregisterReceiver` 쌍?

---

## 상세 규칙

### UI 클래스 네이밍

| 규칙 | 예시 |
|------|------|
| `UI_` 접두어 (예외적으로 언더바 사용) | `UI_Inventory`, `UI_Settings`, `UI_Popup` |

UI 클래스는 일반 클래스와 구분을 위해 예외적으로 `UI_` 접두어 사용.

예외 — UI 헬퍼·컴포넌트: 화면 단위가 아닌 UI 로직 보조 클래스(정렬 컨트롤러, 버추얼라이저, 팝업 전략 객체 등)는 `UI_` 접두어 없이 기능명 그대로 사용한다.

```csharp
// ✅ 화면 단위 — UI_ 접두어 필수
public class UI_Profile : UI_Scene { }
public class UI_ConfirmPopup : UI_Popup { }

// ✅ UI 헬퍼·컴포넌트 — 접두어 생략
public class VirtualizedGridController : MonoBehaviour { }
public class DefaultPopupStrategy : IPopupStrategy { }
```

### UIKeys 중첩 구조

UI 프리팹 키는 `UIKeys` 정적 클래스 내부의 `Scene` / `Popup` / `Sub` 3개 중첩 static class 로 분리한다. `switch/case` 분기에서 사용되므로 `const` 예외 허용 (공통 컨벤션의 상수 규칙 참조).

```csharp
public static class UIKeys
{
    public static class Scene
    {
        public const string UI_LobbyBG = "UI_LobbyBG";
        public const string UI_MainNavigator = "UI_MainNavigator";
    }

    public static class Popup
    {
        public const string UI_MenuPopup = "UI_MenuPopup";
    }

    public static class Sub
    {
        public const string UI_FriendUnit = "UI_FriendUnit";
    }
}
```

사용처에서는 `uiManager.ShowSceneUI<UI_MainNavigator>(UIKeys.Scene.UI_MainNavigator)` 과 같이 중첩 접근.

### UI 아키텍처

#### 소규모 UI — Controller 단일 구조

외부 데이터 의존 없음, UI 간 얽힌 상태 변화 없음, 입력→반영 1단 경로만 가지는 단순 UI 에만 허용.

```csharp
public class SimplePopupController : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Text titleText;

    public void Initialize(string title)
    {
        titleText.text = title;
        closeButton.onClick.AddListener(Close);
    }

    private void Close() { }
}
```

단일 Controller 예외 3조건: 아래 모두 만족 시에만 허용. 적용 시 클래스 상단 주석으로 "외부 데이터·얽힌 상태·다단 경로 없음" 을 명시.

1. 외부 데이터(API / EventManager / DataManager) 상태 없음
2. UI 간 얽힌 상태 변화 없음
3. 입력→반영 1단 경로

예: 뒤로가기 버튼, 정적 배경, 단순 탭. 애매하면 MVC 사용.

#### 대규모 UI — MVC 패턴

```
External ──▶ Controller
              │ 명령
       ┌──────┼──────┐
       ▼      ▼      ▼
     Model  View  Service (선택)
       │      │
       └─구독─┘  (delegate)
```

핵심: View 가 `Bind(model)` 로 Model delegate 를 직접 구독. Controller 는 연결하지 않음.

| 역할 | 책임 | Model 참조 |
|------|------|-----------|
| Controller | 외부 호출 창구, Model/View 생성, `view.Bind(model)`, 명령 전달 | O |
| Model | 데이터 상태 + 비즈니스 로직, delegate 로 변경 알림 | - |
| View | UI 렌더링, `Bind(model)` 로 Model 을 직접 구독 | O |
| Service | Controller 비대화 시 추가 (정렬·필터링 등) | 상황에 따라 |

```csharp
// Controller (UI_Scene)
public class UI_FooList : UI_Scene, IListener<FooEvent>
{
    [Singleton(typeof(EventManager))] private EventManager eventManager;

    private FooModel model;
    private FooView view;

    public override void Init()
    {
        InjectUtil.InjectSingleton(this);
        InjectUtil.InjectComponents(this);

        model = new FooModel();
        view = new FooView();
        view.Initialize(contentRect);
        view.Bind(model);                // View가 Model 직접 구독

        model.OnItemApplied += ApplyItem;
        eventManager.AddListener(this);
    }

    public void OnEvent(FooEvent e, object p = null)
    {
        if (e == FooEvent.ItemSelected)
            model.ToggleEquip(((UI_FooSlot)p).ItemData);
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
public class FooModel
{
    public event Action<Item[], HashSet<string>> OnPageChanged;
    public event Action<string, bool> OnItemApplied;

    public void ToggleEquip(Item item)
    {
        // ...
        OnItemApplied?.Invoke(item.Id, equipped);
    }
}

// View — Bind로 Model 직접 구독
public class FooView
{
    private FooModel model;

    public void Initialize(RectTransform content) { /* contentRect 저장 */ }

    public void Bind(FooModel m)
    {
        model = m;
        m.OnPageChanged += SetData;
    }

    public void Unbind()
    {
        if (model == null) return;
        model.OnPageChanged -= SetData;
        model = null;
    }

    private void SetData(Item[] items, HashSet<string> equipped) { /* 슬롯 갱신 */ }
    public void Release() { Unbind(); }
}

// Service (Controller 비대화 시 추가)
public class FooService
{
    public List<Item> SortByRarity(List<Item> items) { /* ... */ }
}
```

#### Sub UI MVC — 풀링 슬롯

풀에서 꺼낼 때/반환할 때 구독 쌍을 `OnSpawn` / `OnDespawn` 훅에 둔다. `Init` / `Release` 와 혼동 금지.

```csharp
[RequireComponent(typeof(SlotModel), typeof(SlotView))]
public class UI_ItemSlot : UI_Sub
{
    private SlotModel model;
    private SlotView view;

    public override void Init()
    {
        model = GetComponent<SlotModel>();
        view  = GetComponent<SlotView>();
        view.SetModel(model);
    }

    public override void OnSpawn()
    {
        view.SubscribeModel();           // 풀 재사용 시마다 구독
    }

    public override void OnDespawn()
    {
        view.UnsubscribeModel();         // 풀 반환 직전 해제
        model.Reset();                   // 다음 재사용 대비 상태 초기화
    }

    public void SetData(Item item, bool equipped)
    {
        model.SetData(item, equipped);
    }
}
```

### UI 3계층 구조

모든 UI 는 `UI_Base` 를 상속하며, Addressable 로 프리팹을 비동기 로드한다. `UIManager` 가 3가지 유형별로 생성·캐시·해제를 관리한다.

#### UI_Scene (고정 HUD)

- Dictionary 캐싱 (`sceneUICache`)
- `sortOrder` 시작값 0, `DefaultOrderOffset = 1` (`ShowSceneUI<T>` 호출마다 새 인스턴스에만 +1)
- 캐시된 UI 를 재호출하면 기존 order 유지

#### UI_Popup (팝업·확인창)

- Stack(LIFO) 관리 (`popupStack`)
- `sortOrder` = `PopupBaseOrder(=100) + popupStack.Count × PopupOrderIncrement(=10)`
- `CloseTopPopup()` 으로 LIFO 해제, `CloseAllPopups()` 로 전체 해제
- 동일 키가 이미 열려있으면 기존 인스턴스 반환 (중복 오픈 방지)

#### UI_Sub (리스트 아이템·아이콘 — 풀링)

- Queue 기반 오브젝트 풀 (`subUIPools[key]`)
- 부모 UI 는 `GetComponentInParent<UI_Base>()` 로 자동 탐색되어 `ownerSubUIMap` 에 연동 (파생 클래스에서는 `UI_Sub.OwnerUI` 프로퍼티로 접근)
- 부모 UI 가 `Release` 되면 `UIManager.CloseSubUIs(ownerUI)` 로 자동 반환

생명주기 훅:

`UI_Sub` 베이스가 `public virtual` 훅을 제공한다. 파생 클래스는 필요 시 `override`.

```csharp
public abstract class UI_Sub : UI_Base
{
    public virtual void OnSpawn()   { } // 풀에서 꺼낼 때
    public virtual void OnDespawn() { } // 풀에 반환할 때
}
```

- 호출 주체: `UIManager` (`ShowSubUI<T>` / `CloseSubUIs`)
- `Init()` / `Release()` 와 혼동 금지 — `Init` 은 Addressable 로드 직후 1회, `OnSpawn`/`OnDespawn` 은 풀 재사용마다 반복 호출

### IInputEventReceiver 구독 쌍

`IInputEventReceiver` 를 구현한 UI 는 `InputManager.RegisterReceiver` / `UnregisterReceiver` 를 쌍으로 호출한다. EventManager 리스너와 동일한 패턴.

- UI_Scene / UI_Popup: `Init()` 에서 Register, `Release()` 에서 Unregister
- UI_Sub (풀링): `OnSpawn` 에서 Register, `OnDespawn` 에서 Unregister

```csharp
public class UI_MainNavigator : UI_Scene, IInputEventReceiver, IListener<GameStateEvent>
{
    [Singleton(typeof(InputManager))] private InputManager inputManager;
    [Singleton(typeof(EventManager))] private EventManager eventManager;

    public override void Init()
    {
        InjectUtil.InjectSingleton(this);
        InjectUtil.InjectComponents(this);
        inputManager.RegisterReceiver(this);
        eventManager.AddListener(this);
    }

    public void OnInputEvent(InputEvent e, InputEventData d) { /* ... */ }
    public void OnPinchEvent(InputEvent e, PinchData d) { }
    public void OnEvent(GameStateEvent e, object p = null) { /* switch-case */ }

    public override void Release()
    {
        inputManager.UnregisterReceiver(this);
        eventManager.RemoveListener(this);
        base.Release();
    }
}
```

### 베이스 클래스 수정 지양

`UI_Base` / `UI_Popup` / `UI_Sub` 등 공용 베이스는 직접 수정하지 않는다. 기능 확장은 다음 순서로 해결한다.

1. `UIManager` 에 메서드 추가 (가장 우선)
2. 파생 클래스에서 `override`
3. 불가피한 경우에만 리뷰 후 베이스 수정

이유: 베이스 수정은 모든 파생 UI 에 암묵적 영향이 퍼져 회귀 위험이 크다.
