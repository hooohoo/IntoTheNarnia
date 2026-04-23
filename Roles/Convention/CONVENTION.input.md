# Input 컨벤션

적용 대상: `InputHandlerBase` 를 상속하는 클래스, 또는 `IInputEventReceiver` 를 구현하는 클래스.

## 체크리스트

- [ ] `InputHandlerBase` 상속? (`IInputHandler` 직접 구현 금지)

---

## 상세 규칙

### InputHandlerBase 상속 강제

입력 처리 핸들러는 반드시 `InputHandlerBase` 를 상속한다. `IInputHandler` 인터페이스를 직접 구현하지 말 것.

이유: `InputHandlerBase` 가 공통 유틸(`RaycastUI`, `Raycast3D`, `CreateEventData`) 과 기본 `override` 스켈레톤을 제공. `IInputHandler` 를 직접 구현하면 유틸 중복 구현 + Raycast 로직 불일치가 발생한다.

```csharp
// ✅ 권장
public class LobbyInputHandler : InputHandlerBase
{
    [Singleton(typeof(InputManager))]
    private InputManager inputManager;

    public LobbyInputHandler()
    {
        InjectUtil.InjectSingleton(this);
    }

    public override void OnTap(InputData data)
    {
        GameObject hit = RaycastUI(data.position);
        inputManager.BroadcastInputEvent(InputEvent.Tap, CreateEventData(data, hit));
    }
}

// ❌ 금지 — IInputHandler 직접 구현
public class LobbyInputHandler : IInputHandler
{
    public void OnTap(InputData data) { /* ... */ }
    // RaycastUI / CreateEventData 등을 새로 구현해야 함
}
```

### 구현 가이드

- 필요한 콜백만 `override` — 사용하지 않는 콜백은 `override` 자체를 하지 않는다.
- 생성자에서 `InjectUtil.InjectSingleton(this)` 호출 (plain C# class 이므로 생성자 지점).
- `RaycastUI` / `Raycast3D` 로 `hitObject` 획득 후 `BroadcastInputEvent` 로 전파.
- 드래그 상태 변수(예: `dragTarget`) 는 `OnClear()` 에서 초기화.

상세 구조·Raycast 절차·GameState 연동은 `Architecture/ARCHITECTURE.Input.md` 참조.

### IInputEventReceiver 쌍 호출

UI 클래스가 입력을 받으려면 `IInputEventReceiver` 를 구현하고 `RegisterReceiver` / `UnregisterReceiver` 를 쌍으로 호출한다. 상세: `CONVENTION.ui.md` 의 "IInputEventReceiver 구독 쌍" 섹션.
