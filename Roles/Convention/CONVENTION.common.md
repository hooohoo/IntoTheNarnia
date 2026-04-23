# 공통 컨벤션

공통 규칙은 모든 `.cs` 파일에 적용된다. 도메인별 규칙은 같은 폴더의 `CONVENTION.manager.md` / `CONVENTION.ui.md` / `CONVENTION.event.md` / `CONVENTION.data.md` / `CONVENTION.input.md` 를 참조.

## 목표

- 코드 가독성 유지
- IDE/개인 취향으로 인한 충돌 최소화
- 리뷰 시 "스타일 논쟁" 제거

## 체크리스트

각 항목 끝의 `→ 섹션명` 은 모호 판정 시 참조할 아래 "상세 규칙" 의 섹션을 가리킨다.

### 파일 / 네이밍

- [ ] 한 파일 = 한 클래스, 파일명 = 클래스명? → 파일 & 구조
- [ ] 클래스 / enum / public 멤버 / 프로퍼티가 PascalCase? → 네이밍 규칙
- [ ] private 필드가 camelCase이고 `_`, `m_` 접두어 없음? → 네이밍 규칙
- [ ] 상수가 `static readonly`? (`const`는 `UIKeys`만 예외) → 네이밍 규칙
- [ ] 비동기 메서드가 `Async` 접미어 + `UniTask` 반환? → 네이밍 규칙

### 금지 규칙

- [ ] `Update` / `FixedUpdate` / `LateUpdate` 없음? → Unity 특화 규칙
- [ ] 코루틴(`StartCoroutine` / `IEnumerator` / `yield return`) 없음? → Unity 특화 규칙
- [ ] `Camera.main` 직접 사용 없음? → 성능 규칙
- [ ] `transform.Find().GetComponent<>()` / `GetComponent<T>()` 직접 호출 없음? → 성능 규칙
- [ ] `FindObjectOfType` / `GameObject.Find` 없음? → Unity 특화 규칙
- [ ] `Awake` / `Start` 를 프레임워크·씬 초기화 외 용도로 쓰지 않음? → Unity 특화 규칙
- [ ] 람다식 없음? (UnityEvent/DOTween/UniTask 콜백은 예외) → 람다식
- [ ] 델리게이트가 객체 내부 구독만, 외부 체인 없음? → 델리게이트
- [ ] LINQ 메서드 없음? → 성능 규칙
- [ ] `List<T>` / 배열 / 매 프레임 경로는 `for` 루프 기본? → 성능 규칙
- [ ] 매직 넘버 / 매직 스트링 없음? → 매직 넘버 / 매직 스트링 금지
- [ ] `UI_Base` / `UI_Popup` / `UI_Sub` 등 공용 베이스를 직접 수정하지 않음? → 공용 베이스 수정 지양

### 코드 스타일

- [ ] `if` / `for` / `while` 중괄호 규칙 (한 줄 예외만 생략) → 조건문 & switch
- [ ] `var` 는 타입이 명확할 때만? → var 사용
- [ ] `this` 는 필드 충돌 시에만? → this 키워드
- [ ] 문자열 비교에 `==` 사용 (`.Equals()` 지양)? → Unity 특화 규칙
- [ ] 문자열 "값 없음" 체크에 `string.IsNullOrEmpty` (공백 차단 시 `IsNullOrWhiteSpace`)? → Unity 특화 규칙
- [ ] 내부 로직 null 체크 최소화 — DI 주입 필드 / `[FindComponents]` 바인딩 대상 / `static readonly` 상수에 null 가드 없음? → Unity 특화 규칙

### 멤버 정렬

- [ ] 상수 → static → public → SerializeField → private → 프로퍼티 → Unity 콜백 → `Initialize` → public 메서드 → private 메서드 → `Release` 순서? → 멤버 정렬 순서

### 프로젝트 관행

- [ ] `[Singleton(typeof(T))]` + `InjectUtil.InjectSingleton(this)` 로 매니저 주입? → 매니저 주입 규칙
- [ ] 모든 컴포넌트 바인딩에 `[FindComponents("이름"), SerializeField]` + `InjectUtil.InjectComponents(this)` 사용? Component 타입만(GameObject 불가)? → 컴포넌트 바인딩

### 성능 / Unity 관행

- [ ] `SerializeField` 필드는 `private` 선언? (`public` + `SerializeField` 금지) → 네이밍 규칙
- [ ] `GetComponent<T>()` 결과를 필드에 캐싱? → 성능 규칙
- [ ] 매 프레임·빈번 호출 경로에서 `new List<>()` / `new StringBuilder()` 대신 필드 캐싱 + `Clear()` 재사용? → 성능 규칙

### 로깅

- [ ] `private static readonly string LogTag = "[클래스명]";` 선언? → 로깅
- [ ] 모든 로깅을 `LogUtil` 경유? (`Debug.Log` 직접 호출 지양) → 로깅

### 비동기 호출

- [ ] `async UniTask` 메서드 반환값 무시 호출 시 `.Forget()` 표기? → 네이밍 규칙

### Release 정리

- [ ] `Release()` 에서 `EventManager.RemoveListener` 호출 (리스너 구현 시)? → Release 정리 패턴
- [ ] `Release()` 에서 `InputManager.UnregisterReceiver` 호출 (수신자 구현 시)? → Release 정리 패턴
- [ ] `Release()` 에서 delegate `-=` 로 구독 해제? → Release 정리 패턴
- [ ] DOTween 사용 시 `Release()` 에서 `DOKill()`? → Release 정리 패턴
- [ ] 캐시 `Clear()` + 참조 필드 null 처리? → Release 정리 패턴

---

## 상세 규칙

### 기본 원칙

- 코드는 읽히는 것이 최우선
- 자동 포맷 가능한 규칙은 `.editorconfig` 를 따른다
- 스타일보다 의도와 명확성을 중시한다

### 파일 & 구조

- 한 파일 = 한 클래스/구조체/열거형
- 파일명과 클래스(구조체/열거형)명은 동일하게 유지

```csharp
public class PlayerController : MonoBehaviour
{
}
```

### 네이밍 규칙

클래스 / 구조체 / Enum:

| 규칙 | 예시 |
|------|------|
| PascalCase | `PlayerController`, `GameState` |

상수 / static readonly:

| 규칙 | 예시 |
|------|------|
| PascalCase | `private static readonly int MaxRetryCount = 3;` |
| | `private static readonly Vector3 DefaultPosition = Vector3.zero;` |

const vs readonly:

| 항목 | const | readonly |
|------|-------|----------|
| 값 저장 | IL에 인라인 | 필드에 저장 |
| DLL 변경 시 | 참조하는 모든 DLL 재컴파일 필요 | 해당 DLL만 교체하면 됨 |
| 사용 가능 타입 | 기본 타입만 | 모든 타입 |

결론: `static readonly` 권장. 유지보수/버전 관리 측면에서 안전.

```csharp
// ✅ 권장
private static readonly int MaxHP = 100;
private static readonly Vector3 DefaultPosition = Vector3.zero;

// ⚠️ 허용 (외부 라이브러리 호환 필요 시)
public const string Version = "1.0.0";
```

예외 — UIKeys (const 사용): `switch/case` 분기에서 사용되므로 `const` 를 사용한다. 프리팹 이름과 1:1 매칭되는 고정 상수이므로 변경될 가능성이 없다.

```csharp
// ✅ UIKeys - const 허용
public static class UIKeys
{
    public static class Scene
    {
        public const string UI_University = "UI_University";
    }
}
```

public / protected 멤버:

| 규칙 | 예시 |
|------|------|
| PascalCase | `public int MaxHP;` |
| | `public void Move();` |

프로퍼티:

| 규칙 | 예시 |
|------|------|
| PascalCase | `public int Health { get; private set; }` |

private 필드:

| 규칙 | 예시 |
|------|------|
| camelCase | `[SerializeField] private int moveSpeed;` |
| `_`, `m_` 접두어 사용 ❌ | `private bool isGrounded;` |

Unity Inspector 및 SerializeField 와의 자연스러운 호환을 우선함.

이벤트 / 델리게이트:

| 규칙 | 예시 |
|------|------|
| On + 동사 | `public event Action OnPlayerDeath;` |
| | `public event Action<int> OnScoreChanged;` |

이벤트 파라미터 (EventManager): 값 타입 파라미터가 2개 이상일 때 `object[]` 로 넘기면 각각 boxing 되어 파라미터 수만큼 Object Header(16바이트)가 중복 할당됨. class 로 묶어서 전달할 것. 상세: `CONVENTION.event.md`.

비동기 메서드 (UniTask):

| 규칙 | 예시 |
|------|------|
| Async 접미어 | `public async UniTask LoadDataAsync()` |

비동기 메서드를 반환값 무시로 호출할 때는 반드시 `.Forget()` 을 표기한다. 생략하면 컴파일러 경고 + 예외 무시 위험.

```csharp
// ❌ 반환값 무시 호출 — 경고 + 예외 묻힘
LoadDataAsync();

// ✅ Fire-and-Forget 명시
LoadDataAsync().Forget();
```

UI 클래스:

| 규칙 | 예시 |
|------|------|
| `UI_` 접두어 (예외적으로 언더바 사용) | `UI_Inventory`, `UI_Settings`, `UI_Popup` |

상세 및 UI 헬퍼·컴포넌트 예외: `CONVENTION.ui.md`.

### 메서드 스타일

- 메서드는 기본적으로 block body 권장
- 한 줄이고 의미가 명확한 경우만 expression-bodied 허용

```csharp
// 권장
void Jump()
{
    ApplyForce();
}

// 허용
bool IsAlive() => hp > 0;
```

### 조건문 & switch

if / for / while:

- 기본적으로 중괄호 사용
- 한 줄일 경우 중괄호 생략 허용

```csharp
// 권장
if (isDead)
{
    return;
}

// 허용 (한 줄)
if (isDead)
    return;
```

switch:

- 기본은 여러 줄
- 한 줄 case 는 가독성 유지 시 허용

```csharp
switch (state)
{
    case State.Idle:
        UpdateIdle();
        break;

    case State.Jump: UpdateJump(); break;
}
```

### var 사용

타입이 명확한 경우에만 허용.

```csharp
var count = 10;
var list = new List<int>();
```

반환 타입이 불명확한 경우 사용 금지.

```csharp
var data = GetData(); // ❌
```

### this 키워드

필드와 매개변수 이름이 충돌할 때만 사용.

```csharp
public void SetHP(int hp)
{
    this.hp = hp;
}
```

### 람다식

- 기본적으로 사용 금지
- 디버깅 시 브레이크포인트 해제 불가 문제
- 외부 라이브러리 / 공식 문서 가이드를 따를 때만 허용

```csharp
// ❌ 사용 금지
list.Where(x => x.IsActive).ToList();

// ✅ 허용 (외부 라이브러리 사용 시)
DOTween.Sequence().OnComplete(() => OnAnimationEnd());
UniTask.WhenAll(tasks);
```

### 델리게이트

- 해당 객체 내부에서만 사용
- 외부로 전파 시 스파게티 코드 원인

```csharp
// ✅ 올바른 사용 - 내부 구독
public class PlayerHealth : MonoBehaviour
{
    public event Action OnDeath;

    private void Die()
    {
        OnDeath?.Invoke();
    }
}

// ❌ 잘못된 사용 - 외부에서 체인 연결
playerA.OnDeath += playerB.OnAllyDeath;
playerB.OnAllyDeath += playerC.OnTeamEvent; // 스파게티
```

### 제네릭

기본 원칙:

- 가급적 일반 메서드로 구현 (제네릭 필요 시 클래스보다 메서드 선호)
- 프레임워크 레벨 제외, 성능 이슈로 남용 금지

사용 범위:

- 해당 클래스 / 객체 내부에서만 사용 (프레임워크 제외)
- 외부로 전파 시 스파게티 코드 원인

```csharp
// ✅ 내부에서만 사용
public class Inventory
{
    private T FindItem<T>(List<T> items, int id) where T : IItem
    {
        foreach (var item in items)
        {
            if (item.Id == id) return item;
        }
        return default;
    }
}

// ❌ 외부에 복잡한 제네릭 노출
public T GetComponent<T, U, V>() where T : class where U : struct where V : IDisposable
```

외부 노출 시 규칙:

- 사용하기 쉽게 단순화
- 타입 제약이 명확해야 함

```csharp
// ✅ 외부 노출 - 단순하고 명확하게
public T Get<T>() where T : Component
{
    return GetComponent<T>();
}

// ✅ 필요시 비제네릭 오버로드 제공
public Component Get(Type type)
{
    return GetComponent(type);
}
```

### 멤버 정렬 순서

```csharp
public class Example : MonoBehaviour
{
    // 1. 상수 (readonly 권장)
    // 2. static 필드
    // 3. public 필드
    // 4. SerializeField 필드
    // 5. private 필드
    // 6. 프로퍼티
    // 7. Unity 콜백 (Awake, Start) - 프레임워크/씬 초기화만
    // 8. Initialize 메서드
    // 9. public 메서드
    // 10. private 메서드
    // 11. Release 메서드
}
```

### Unity 특화 규칙

라이프사이클 메서드:

- `Update()`, `FixedUpdate()`, `LateUpdate()` 사용 금지
- `Awake()`, `Start()` 는 프레임워크/씬 초기화에만 허용
- 일반 객체는 `Initialize()` 메서드로 직접 구현

```csharp
// ✅ 허용 - 프레임워크/씬 레벨 초기화
public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        InitializeServices();
        InitializeManagers();
    }
}

public class SceneInitializer : MonoBehaviour
{
    private void Start()
    {
        LoadSceneData();
    }
}

// ✅ 일반 객체 - Initialize로 직접 호출
public class PlayerController : MonoBehaviour
{
    public void Initialize(PlayerData data)
    {
        // 외부에서 명시적으로 호출
    }
}

// ❌ 잘못된 사용 - 일반 객체에서 Awake 사용
public class PlayerController : MonoBehaviour
{
    private void Awake()
    {
        // 일반 객체는 Awake 사용 금지
    }
}
```

코루틴 & 비동기:

- 코루틴 사용 금지
- UniTask 사용

```csharp
public async UniTask FadeOutAsync()
{
    await UniTask.Delay(1000);
}
```

Null 체크:

- 외부 API 호출 결과만 null 체크
- 내부 로직에서는 null 체크 최소화

```csharp
// API 호출 - null 체크 O
var response = await api.GetDataAsync();
if (response == null) return;

// 내부 로직 - null 체크 X (설계로 보장)
player.TakeDamage(10);
```

설계로 유효성이 보장되어 가드를 두지 않는 대상:

- `[Singleton(typeof(T))]` + `InjectUtil.InjectSingleton` 주입 필드
- `[FindComponents]` + `InjectUtil.InjectComponents` 바인딩 대상
- `static readonly` 상수

반대로 가드를 유지하는 대상:

- 외부 API / 플랫폼 브릿지 응답
- 사용자 입력 문자열 (`string.IsNullOrEmpty` 사용)
- 수명이 분기되는 `CancellationTokenSource` 등 명시적으로 미사용 상태가 정상인 필드

문자열 "값 없음" 체크: 문자열 파라미터의 "값 없음" 판정은 `!= null` / `== null` 대신 `string.IsNullOrEmpty` 를 사용한다. 공백 문자(`""`, `"   "`)가 실수로 흘러들어 "빈 버튼이 활성화된 채 표시" 같은 사고가 발생하기 때문.

```csharp
// ✅ 권장
if (string.IsNullOrEmpty(title)) return;

// ⚠️ 지양 — 빈 문자열이 필터링되지 않음
if (title != null) { /* ... */ }

// ✅ 공백 문자(예: "   ")까지 차단해야 할 땐
if (string.IsNullOrWhiteSpace(userInput)) return;
```

예외: 빈 문자열을 의도적으로 "값 있음" 으로 구분해야 하는 도메인 특수 케이스(사용자가 공백 입력을 명시적으로 선택한 경우 등)에만 `!= null` 유지.

문자열 비교: C# 의 `==` 연산자는 문자열의 값 비교이며 `null`-safe 하다. Java 습관으로 `.Equals()` 를 사용하면 좌변이 null 일 때 NRE 위험이 있으므로 지양한다.

```csharp
// ✅ 권장
if (state == "Idle") { }
if (UIKey == UIKeys.Popup.UI_Confirm) { }

// ⚠️ NRE 위험
if (state.Equals("Idle")) { }
```

기타:

- `SerializeField private` 우선 사용
- `FindObjectOfType` / `GameObject.Find` 사용 최소화
- `Debug.Log` 직접 호출은 지양 (상세: 아래 "로깅" 섹션)
- `Camera.main` 직접 사용 금지 — `CameraManager.MainCamera` 를 통해 접근 (상세: 아래 "성능 규칙" 섹션)

### 공용 베이스 수정 지양

`UI_Base`, `UI_Popup`, `UI_Sub`, `Singleton<T>` 등 공용 베이스 클래스는 직접 수정하지 않는다. 기능 확장은 다음 순서로 해결한다.

1. 담당 Manager(UIManager 등)에 메서드 추가 (가장 우선)
2. 파생 클래스에서 `override`
3. 불가피한 경우에만 리뷰 후 베이스 수정

이유: 베이스 수정은 모든 파생 클래스에 암묵적 영향이 퍼져 회귀 위험이 크다. UI 베이스 관련 상세는 `CONVENTION.ui.md` 참조.

### 매직 넘버 / 매직 스트링 금지

```csharp
// ❌ 매직 넘버
if (hp <= 0)
if (speed > 10f)
await UniTask.Delay(1000);

// ✅ 상수로 의미 부여
private static readonly int MinHP = 0;
private static readonly float MaxSpeed = 10f;
private static readonly int FadeDelayMs = 1000;

if (hp <= MinHP)
if (speed > MaxSpeed)
await UniTask.Delay(FadeDelayMs);
```

```csharp
// ❌ 매직 스트링
gameObject.tag == "Player"
Animator.SetTrigger("Jump")
PlayerPrefs.GetInt("HighScore")

// ✅ 상수 클래스로 관리
public static class Tags
{
    public static readonly string Player = "Player";
    public static readonly string Enemy = "Enemy";
}

public static class AnimParams
{
    public static readonly string Jump = "Jump";
    public static readonly string Run = "Run";
}

public static class PrefKeys
{
    public static readonly string HighScore = "HighScore";
}

gameObject.tag == Tags.Player
Animator.SetTrigger(AnimParams.Jump)
PlayerPrefs.GetInt(PrefKeys.HighScore)
```

### Prefab / Asset 네이밍

| 분류 | 규칙 | 예시 |
|------|------|------|
| Prefab | `{기능}_{상태/타입}.prefab` | `Player_Idle.prefab`, `Enemy_Boss.prefab` |
| Sprite | `{카테고리}_{이름}.png` | `Icon_Coin.png`, `UI_Button_Primary.png` |
| Audio | `{타입}_{이름}.wav` | `SFX_Jump.wav`, `BGM_Main.mp3` |
| Material | `Mat_{이름}.mat` | `Mat_Player.mat`, `Mat_Ground.mat` |
| Animation | `{대상}@{동작}.anim` | `Player@Run.anim`, `Enemy@Attack.anim` |

### 인터페이스 활용

- 외부 의존성(API, DB, 파일 등)은 인터페이스로 추상화
- 테스트 시 Mock 교체 용이

```csharp
// ✅ 인터페이스 정의
public interface IDataService
{
    UniTask<PlayerData> LoadAsync();
    UniTask SaveAsync(PlayerData data);
}

// ✅ 실제 구현
public class FirebaseDataService : IDataService
{
    public async UniTask<PlayerData> LoadAsync() { }
    public async UniTask SaveAsync(PlayerData data) { }
}

// ✅ 사용 - 인터페이스에 의존
public class PlayerManager
{
    private readonly IDataService dataService;

    public PlayerManager(IDataService dataService)
    {
        this.dataService = dataService;
    }
}
```

### 성능 규칙

GC 최소화:

```csharp
// ❌ 매 프레임 할당
void Update()
{
    var list = new List<int>();  // 매번 GC 발생
    var text = "Score: " + score;  // string 할당
}

// ✅ 캐싱 / 재사용
private List<int> cachedList = new();
private StringBuilder sb = new();

void UpdateScore()
{
    cachedList.Clear();

    sb.Clear();
    sb.Append("Score: ").Append(score);
}
```

박싱 방지:

```csharp
// ❌ 박싱 발생
object value = 10;  // int → object
string.Format("{0}", 10);  // 박싱

// ✅ 제네릭 사용
void Process<T>(T value) where T : struct { }
$"Score: {score}"  // 보간 문자열
```

오브젝트 풀링:

```csharp
// ❌ 반복 생성/파괴
Instantiate(bulletPrefab);
Destroy(bullet);

// ✅ 풀링 사용
var bullet = bulletPool.Get();
bulletPool.Release(bullet);
```

LINQ 사용 금지:

```csharp
// ❌ LINQ 사용 금지 - GC 발생, 디버깅 어려움
var activeItems = items.Where(x => x.IsActive).ToList();
var firstEnemy = enemies.FirstOrDefault(e => e.HP > 0);
var totalScore = scores.Sum();

// ✅ 반복문으로 직접 구현
var activeItems = new List<Item>();
foreach (var item in items)
{
    if (item.IsActive)
    {
        activeItems.Add(item);
    }
}

Enemy firstEnemy = null;
foreach (var enemy in enemies)
{
    if (enemy.HP > 0)
    {
        firstEnemy = enemy;
        break;
    }
}

int totalScore = 0;
foreach (var score in scores)
{
    totalScore += score;
}
```

foreach 자제: 큰 컬렉션을 반복할 때 foreach 는 일부 컬렉션에서 IEnumerator 할당이나 박싱을 유발할 수 있다. index 기반 `for` 루프를 우선한다.

```csharp
// ✅ 권장 — index 기반 for
for (int i = 0; i < items.Count; i++)
{
    var item = items[i];
    // ...
}

// ⚠️ 대규모 순회에서 지양
foreach (var item in items) { /* ... */ }
```

예외: 컬렉션 크기가 작거나 IEnumerable 만 노출되는 API 에서는 foreach 허용.

GetComponent 사용 지양:

- `GetComponent<T>()` / `transform.Find().GetComponent<>()` 직접 호출 금지.
- 컴포넌트 참조는 반드시 `[FindComponents("이름"), SerializeField]` + `InjectUtil.InjectComponents(this)` 패턴 사용 (상세: 아래 "컴포넌트 바인딩" 섹션).
- 불가피하게 직접 호출해야 하면 필드에 캐싱 필수 — 매 호출 시 트리 탐색 비용 발생.

**Why:** 프로젝트 전체가 `[FindComponents]` 패턴을 표준으로 채택 중이며 일관성 유지가 핵심. `transform.Find()` 는 이름 하드코딩·탐색 비용·오타 런타임 실패 리스크가 크다.

**How to apply:** `MonoBehaviour` 든 `UI_Base` 상속이든, 자식 오브젝트 컴포넌트를 참조할 때는 항상 `[FindComponents]` + `InjectUtil.InjectComponents` 사용.

Camera.main 사용 금지:

- `Camera.main` 직접 사용 금지
- CameraManager 를 통해 접근

```csharp
// ❌ 직접 사용 금지
Camera.main.ScreenToWorldPoint(pos);
Camera.main.orthographicSize = 5f;

// ✅ CameraManager 사용
[Singleton(typeof(CameraManager))]
private CameraManager cameraManager;

private void Start()
{
    InjectUtil.InjectSingleton(this);
}

private void Example()
{
    cameraManager.MainCamera.ScreenToWorldPoint(pos);
    cameraManager.MainCamera.orthographicSize = 5f;
}
```

이유: Camera.main 은 내부에서 FindWithTag 를 호출하여 매번 검색 비용 발생.

### 주석 스타일

```csharp
/// <summary>
/// XML 문서 주석은 public API에만 사용
/// </summary>
public void PublicMethod() { }

// 일반 주석은 "왜"를 설명 (무엇을 하는지는 코드로)
private void InternalMethod() { }
```

### 네임스페이스

- 기본: 생략
- 충돌 시에만 1단계 사용

```csharp
// ✅ 기본 - 네임스페이스 생략
public class PlayerController : MonoBehaviour
{
}

// ✅ 충돌 시 - 1단계 네임스페이스
namespace Player
{
    public class Controller : MonoBehaviour { }
}

namespace Enemy
{
    public class Controller : MonoBehaviour { }
}
```

### 자동 포맷 & 도구

- `.editorconfig` 는 프로젝트 기준을 따른다
- VSCode 사용자는 다음 확장 설치 권장:
  - EditorConfig for VS Code
  - C# (Microsoft)

### 매니저 주입 규칙

본 섹션부터 아래는 프로젝트 아키텍처에서 확립된 코드 작성 관행이다. 매니저·UI·데이터 계층 전체 구조는 `Architecture/ARCHITECTURE.md` 참조.

원칙:

매니저 접근은 `[Singleton(typeof(T))]` 어트리뷰트 + `InjectUtil.InjectSingleton(this)` 호출로 수행한다. 일반 사용처에서 `XxxManager.Instance` 직접 접근은 지양한다.

- Manager 의 `Init()` 첫 줄에서 `InjectUtil.InjectSingleton(this)` 호출
- plain C# 클래스(MonoBehaviour 아님)에서도 동일 패턴 사용 가능 — 생성자 또는 `Initialize()` 첫 줄에서 호출
- 매니저 간 순환 의존 금지 — `DependencyResolver` 가 위상 정렬(Kahn's Algorithm)로 초기화 순서를 결정함

예시 (Manager):

```csharp
public partial class DataManager : Singleton<DataManager>, IManager
{
    private static readonly string LogTag = "[DataManager]";

    [Singleton(typeof(AddressableManager))]
    private AddressableManager addressableManager;

    public void Init()
    {
        InjectUtil.InjectSingleton(this);
        // ... 도메인 초기화
    }
}
```

예시 (plain C# class — 생성자 지점):

```csharp
public class InventoryService
{
    [Singleton(typeof(EventManager))]
    private EventManager eventManager;

    public InventoryService()
    {
        InjectUtil.InjectSingleton(this);
    }
}

public class LobbyInputHandler : InputHandlerBase
{
    [Singleton(typeof(InputManager))]
    private InputManager inputManager;

    public LobbyInputHandler()
    {
        InjectUtil.InjectSingleton(this);
    }
}
```

예외 — 공용 베이스 클래스의 정적 접근: 공용 베이스(`UI_Popup`, `UI_Scene` 등)에서 필드 주입이 어려운 경우에만 `UIManager.Instance.CloseSubUIs(this)` 같은 정적 접근을 사용한다. (현 프로젝트의 `UI_Popup.Release()`, `UI_Scene.Release()` 구현 참고) 일반 파생/사용처에서는 반드시 DI 주입 방식을 따른다.

### 컴포넌트 바인딩

원칙:

자식 오브젝트의 컴포넌트 참조는 `[FindComponents("이름"), SerializeField]` 어트리뷰트로 선언하고, 초기화 지점(`Init()` / `Initialize()` / `Awake()`)에서 `InjectUtil.InjectComponents(this)` 를 호출해 자동 바인딩한다. MonoBehaviour, UI_Base 상속 여부 무관하게 모든 컴포넌트 바인딩에 적용.

- `transform.Find().GetComponent<>()` / `GetComponentInChildren<>()` 직접 호출 금지
- Component 타입만 바인딩 가능 — `GameObject` 타입 필드에는 사용 불가. 활성/비활성 제어가 필요하면 바인딩한 컴포넌트의 `.gameObject` 프로퍼티로 접근.
- 단일 컴포넌트·배열·`List<T>` 모두 지원
- `[SerializeField]` 와 동시 사용 가능 — Inspector 가시성을 유지하면서 자동 바인딩

**Why:** 프로젝트 전체가 이 패턴을 사용하며 일관성 유지가 중요. MainTab 등 모든 UI 클래스에서 동일한 패턴 적용.

**How to apply:** MonoBehaviour 든 UI_Base 상속이든, 자식 오브젝트 컴포넌트를 참조할 때는 항상 `[FindComponents]` + `InjectUtil.InjectComponents` 사용.

생성자 오버로드: `FindComponentsAttribute` 는 2개의 생성자를 제공한다. 둘 다 `params string[]` 을 받으므로 한 필드에 여러 이름을 나열하면 배열/`List<T>` 로 한 번에 바인딩된다.

| 생성자 시그니처 | 탐색 스코프 | 용도 |
|---|---|---|
| `[FindComponents(params string[] names)]` | `BindRoot` 태그 루트 → 하위 전체 재귀 | 기본. 프리팹 어디에 있든 이름으로 탐색 |
| `[FindComponents(bool findChild, params string[] names)]` | `findChild=true` 면 현재 스크립트의 자식 스코프만 | 동일 이름이 여러 곳에 있을 때 (Slot 등) |

```csharp
// 단일 이름
[FindComponents("BG"), SerializeField]
private Image bg;

// 복수 이름 — 배열 / List<T>로 한 번에 바인딩
[FindComponents("Slot1", "Slot2", "Slot3"), SerializeField]
private Image[] slots;

// 자식 스코프 제한
[FindComponents(true, "Icon"), SerializeField]
private Image iconInThisSlot;
```

예시:

```csharp
public class UI_Profile : UI_Scene
{
    [FindComponents("BG"), SerializeField]
    private Image bg;

    [FindComponents("NameText"), SerializeField]
    private TextMeshProUGUI nameText;

    [FindComponents("Level"), SerializeField]
    private Image levelBase;

    public override void Init()
    {
        InjectUtil.InjectComponents(this);
    }

    public override void Release() { }
}
```

주의:

- `GameObject` 타입 필드에 `[FindComponents]` 를 붙이면 바인딩되지 않는다. 반드시 `Component` 파생 타입(`Image`, `Button`, `RectTransform`, `TextMeshProUGUI` 등)을 사용.
- 루트 탐색 시에는 프리팹 최상단에 `BindRoot` 태그가 설정돼 있어야 한다.

### 로깅

모든 클래스는 식별용 `LogTag` 상수를 선언하고 로깅은 `LogUtil` 을 경유한다. `UnityEngine.Debug.Log` 직접 호출은 지양.

```csharp
public class LobbyState : IGameState
{
    private static readonly string LogTag = "[LobbyState]";

    public void OnStateEnter()
    {
        LogUtil.Log($"{LogTag} 진입");
        // ...
    }
}
```

- `LogTag` 는 `private static readonly string` 으로 선언, 값은 `"[클래스명]"` 형식.
- `LogUtil` 을 통해 일관된 포맷·빌드별 필터링이 가능.
- `Debug.Log` 직접 호출은 리뷰 기준 지적 대상.

### Release 정리 패턴

`Release()` / `OnDisable()` 등 해제 지점에서는 아래 항목을 통합 정리해야 한다. 누락 시 파괴된 객체가 계속 호출되어 NRE 또는 메모리 누수 발생.

정리 대상:

- EventManager 리스너 — `eventManager.RemoveListener<TEnum>(this)` (리스너 구현 시)
- InputManager 수신자 — `inputManager.UnregisterReceiver(this)` (`IInputEventReceiver` 구현 시)
- delegate 구독 — `-=` 연산자로 반드시 해제
- DOTween 트윈 — `this.DOKill()` 또는 대상 오브젝트의 `DOKill()`
- 캐시 컬렉션 — `Clear()` + 참조 필드 `null` 처리
- CancellationTokenSource — `Cancel()` + `Dispose()` + `null` 처리

```csharp
public class UI_Foo : UI_Scene, IListener<FooEvent>, IInputEventReceiver
{
    [Singleton(typeof(EventManager))] private EventManager eventManager;
    [Singleton(typeof(InputManager))] private InputManager inputManager;

    private FooModel model;

    public override void Init()
    {
        InjectUtil.InjectSingleton(this);
        eventManager.AddListener<FooEvent>(this);
        inputManager.RegisterReceiver(this);
        model.OnChanged += HandleChanged;
    }

    public override void Release()
    {
        eventManager.RemoveListener<FooEvent>(this);
        inputManager.UnregisterReceiver(this);
        model.OnChanged -= HandleChanged;
        this.DOKill();
        base.Release();
    }
}
```

쌍 호출 위치:

- 일반 MonoBehaviour — `OnEnable`/`OnDisable` 쌍
- `UI_Scene` / `UI_Popup` 파생 — `Init()` / `Release()` 쌍 (`UIManager` 주도 생명주기를 이미 갖고 있어 Unity 콜백을 중복 사용할 필요 없음)
- plain C# class — `Initialize()` / `Release()` 쌍

---

## 마무리 원칙

이 문서는 "강제 규칙" 이 아니라 "팀의 합의 기준" 이다. 리뷰 시 스타일보다 로직과 안정성을 우선한다.
