# Event 컨벤션

적용 대상: `eventManager.PostNotification` / `eventManager.Enqueue` 호출을 포함한 파일, 또는 `IListener<T>` 를 구현하는 클래스.

## 체크리스트

- [ ] 이벤트 파라미터 2개 이상 시 `object[]` 대신 전용 class 로 묶고 인스턴스 캐싱·재사용?
- [ ] `Enqueue` 사용 시 대응하는 `ProcessQueue` 호출 지점 존재?
- [ ] Event Enum 파일 경로가 `Assets/@Project/01.Scripts/Enum/Event/{Feature}Event.cs`?
- [ ] `IListener<T>` 구현 시 `AddListener` / `RemoveListener` 쌍 호출? (MonoBehaviour 는 OnEnable/OnDisable, UI_Scene/UI_Popup 파생은 Init/Release)

---

## 상세 규칙

### Enum 파일 경로

이벤트 타입은 도메인별 Enum 으로 분리하고, 파일은 `Assets/@Project/01.Scripts/Enum/Event/{Feature}Event.cs` 에 둔다.

예: `SceneEvent`, `GameStateEvent`, `InputEvent`, `HexGridEvent`, `FlutterEvent`, `WebEvent`, `LayoutEvent`, `CostumeEvent` 등. 전체 목록은 해당 폴더 참조.

```csharp
// Assets/@Project/01.Scripts/Enum/Event/HexGridEvent.cs
public enum HexGridEvent
{
    GridLoaded,
    GridCleared,
    BlockClicked,
    BlockHighlighted,
    PathFound,
}
```

### 리스너 구현

이벤트 구독 대상은 `IListener<TEnum>` 인터페이스를 구현한다.

```csharp
public class UI_MainNavigator : UI_Scene, IListener<GameStateEvent>
{
    [Singleton(typeof(EventManager))] private EventManager eventManager;

    public override void Init()
    {
        InjectUtil.InjectSingleton(this);
        InjectUtil.InjectComponents(this);

        eventManager.AddListener<GameStateEvent>(this);
    }

    public override void Release()
    {
        eventManager.RemoveListener<GameStateEvent>(this);
    }

    public void OnEvent(GameStateEvent eventType, object param)
    {
        // 이벤트 처리
    }
}
```

### 발행 방식 2가지

- 동기: `eventManager.PostNotification<TEnum>(eventType, param)` — 즉시 모든 리스너 호출
- 지연: `eventManager.Enqueue<TEnum>(eventType, param)` 후 `ProcessQueue<TEnum>()` 호출 — 프레임/단계 분리가 필요한 경우

`Enqueue` 를 사용하면 반드시 대응하는 `ProcessQueue` 호출 지점이 명시적으로 존재해야 한다. 누락 시 이벤트가 영영 처리되지 않고 메모리에 적체된다.

### 동작 규칙

- 리스너는 역순(LIFO) 으로 호출됨 — 나중에 등록된 리스너가 먼저 호출됨
- MonoBehaviour 리스너는 `gameObject.activeInHierarchy` 가 false 면 건너뜀 (내부 자동 처리)
- 리스너 내부 예외는 로그로 남기고 다음 리스너 호출을 계속함

### 구독/해제 쌍 호출

리스너는 반드시 `AddListener`/`RemoveListener` 를 쌍으로 호출한다. 해제 누락 시 파괴된 객체가 계속 호출되어 NRE 가 발생한다.

- 일반 MonoBehaviour — `OnEnable`/`OnDisable` 쌍
- `UI_Scene` / `UI_Popup` 파생 — `Init()` / `Release()` 쌍 (`UIManager` 주도 생명주기를 이미 갖고 있어 Unity 콜백을 중복 사용할 필요가 없다. 파생 예시는 위 "리스너 구현" 코드 참고)

공통 Release 정리 패턴 전체(EventManager + InputManager + delegate + DOTween 등)는 `CONVENTION.common.md` 의 "Release 정리 패턴" 섹션 참조.

### 이벤트 파라미터 — object[] 대신 클래스로 묶고 캐싱

`PostNotification` / `Enqueue` 의 `param` 은 `object` 타입이다. 값 타입(int, float, struct 등) 2개 이상을 `object[]` 로 넘기면 박싱과 배열 할당이 매 발행마다 발생한다. 또한 `struct` 를 param 으로 넘겨도 `object` 로 전달되는 순간 박싱이 일어나 이점이 없다.

권장:

- 파라미터가 2개 이상이면 전용 class 로 묶어 타입 안정성을 확보한다.
- 해당 class 인스턴스는 캐싱·재사용하여 매 발행 시 `new` 할당을 피한다. (필드는 발행 직전에 채워 넣기)

```csharp
// ❌ 박싱 + 배열 할당 매번 발생
eventManager.PostNotification(FooEvent.Bar, new object[] { id, value, flag });

// ✅ 전용 class + 캐싱 재사용
public class BarParam
{
    public int Id;
    public int Value;
    public bool Flag;
}

// 호출부에서 1회만 생성하여 보관
private readonly BarParam barParam = new();

private void FireBar(int id, int value, bool flag)
{
    barParam.Id = id;
    barParam.Value = value;
    barParam.Flag = flag;
    eventManager.PostNotification(FooEvent.Bar, barParam);
}
```

주의: 캐싱한 param 인스턴스를 리스너가 비동기로 보관하면 다음 발행에서 값이 덮어써진다. 리스너는 받은 즉시 필요한 값을 지역 변수로 복사하거나, 비동기 보관이 필요하다면 발행마다 새 인스턴스를 사용한다.

`Enqueue` 와 캐싱 파라미터 조합 주의: `Enqueue` 에 캐싱 인스턴스를 넣고 flush 전에 동일 인스턴스 필드를 다시 덮어쓰면 큐에 들어간 항목도 같은 참조이므로 최종 값으로 뭉개진다. `Enqueue` 로 쓸 때는 매 호출마다 new 하거나, 큐 용량만큼 전용 파라미터 풀을 돌려 쓰도록 설계.
