# 📘 Unity Coding Convention

## 🎯 목표

- 코드 가독성 유지
- IDE/개인 취향으로 인한 충돌 최소화
- 리뷰 시 "스타일 논쟁" 제거

---

## 1. 기본 원칙

- 코드는 읽히는 것이 최우선
- 자동 포맷 가능한 규칙은 `.editorconfig`를 따른다
- 스타일보다 의도와 명확성을 중시한다

---

## 2. 파일 & 구조

- 한 파일 = 한 클래스
- 파일명과 클래스명은 동일하게 유지
- MonoBehaviour는 반드시 PascalCase 클래스명 사용

```csharp
public class PlayerController : MonoBehaviour
{
}
```

---

## 3. 네이밍 규칙

### 클래스 / 구조체 / Enum
| 규칙 | 예시 |
|------|------|
| PascalCase | `PlayerController`, `GameState` |

### 상수 / static readonly
| 규칙 | 예시 |
|------|------|
| PascalCase | `private static readonly int MaxRetryCount = 3;` |
| | `private static readonly Vector3 DefaultPosition = Vector3.zero;` |

#### const vs readonly

| 항목 | const | readonly |
|------|-------|----------|
| 값 저장 | IL에 인라인 | 필드에 저장 |
| DLL 변경 시 | 참조하는 모든 DLL 재컴파일 필요 | 해당 DLL만 교체하면 됨 |
| 사용 가능 타입 | 기본 타입만 | 모든 타입 |

**결론**: `static readonly` 권장 (유지보수/버전 관리 측면에서 안전)

```csharp
// ✅ 권장
private static readonly int MaxHP = 100;
private static readonly Vector3 DefaultPosition = Vector3.zero;

// ⚠️ 허용 (외부 라이브러리 호환 필요 시)
public const string Version = "1.0.0";
```

#### 예외: UIKeys (const 사용)

UIKeys는 `switch/case` 분기에서 사용되므로 `const`를 사용한다.
프리팹 이름과 1:1 매칭되는 고정 상수이므로 변경될 가능성이 없다.

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

### public / protected 멤버
| 규칙 | 예시 |
|------|------|
| PascalCase | `public int MaxHP;` |
| | `public void Move();` |

### 프로퍼티
| 규칙 | 예시 |
|------|------|
| PascalCase | `public int Health { get; private set; }` |

### private 필드
| 규칙 | 예시 |
|------|------|
| camelCase | `[SerializeField] private int moveSpeed;` |
| `_`, `m_` 접두어 사용 ❌ | `private bool isGrounded;` |

> Unity Inspector 및 SerializeField와의 자연스러운 호환을 우선함

### 이벤트 / 델리게이트
| 규칙 | 예시 |
|------|------|
| On + 동사 | `public event Action OnPlayerDeath;` |
| | `public event Action<int> OnScoreChanged;` |

### 비동기 메서드 (UniTask)
| 규칙 | 예시 |
|------|------|
| Async 접미어 | `public async UniTask LoadDataAsync()` |

### UI 클래스
| 규칙 | 예시 |
|------|------|
| `UI_` 접두어 (예외적으로 언더바 사용) | `UI_Inventory`, `UI_Settings`, `UI_Popup` |

> UI 클래스는 일반 클래스와 구분을 위해 예외적으로 `UI_` 접두어 사용

---

## 4. 메서드 스타일

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

---

## 5. 조건문 & switch

### if / for / while
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

### switch
- 기본은 여러 줄
- 한 줄 case는 가독성 유지 시 허용

```csharp
switch (state)
{
    case State.Idle:
        UpdateIdle();
        break;

    case State.Jump: UpdateJump(); break;
}
```

---

## 6. var 사용

- 타입이 명확한 경우에만 허용

```csharp
var count = 10;
var list = new List<int>();
```

- 반환 타입이 불명확한 경우 사용 ❌

```csharp
var data = GetData(); // ❌
```

---

## 7. this 키워드

- 필드와 매개변수 이름이 충돌할 때만 사용

```csharp
public void SetHP(int hp)
{
    this.hp = hp;
}
```

---

## 8. 람다식 (Lambda)

- **기본적으로 사용 금지**
- 디버깅 시 브레이크포인트 해제 불가 문제
- 외부 라이브러리 / 공식 문서 가이드를 따를 때만 허용

```csharp
// ❌ 사용 금지
list.Where(x => x.IsActive).ToList();

// ✅ 허용 (외부 라이브러리 사용 시)
DOTween.Sequence().OnComplete(() => OnAnimationEnd());
UniTask.WhenAll(tasks);
```

---

## 9. 델리게이트 (Delegate / Event)

- **해당 객체 내부에서만 사용**
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

---

## 10. 제네릭 (Generic)

### 기본 원칙
- 가급적 **일반 메서드**로 구현 (제네릭 필요 시 클래스보다 메서드 선호)
- 프레임워크 레벨 제외, 성능 이슈로 남용 금지

### 사용 범위
- 해당 클래스 / 객체 **내부에서만** 사용 (프레임워크 제외)
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

### 외부 노출 시 규칙
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

---

## 11. 멤버 정렬 순서

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

---

## 12. Unity 특화 규칙

### 라이프사이클 메서드
- `Update()`, `FixedUpdate()`, `LateUpdate()` 사용 ❌
- `Awake()`, `Start()`는 **프레임워크/씬 초기화에만** 허용
- 일반 객체는 `Initialize()` 메서드로 직접 구현

```csharp
// ✅ 허용 - 프레임워크/씬 레벨 초기화
public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        // 프레임워크 초기화
        InitializeServices();
        InitializeManagers();
    }
}

public class SceneInitializer : MonoBehaviour
{
    private void Start()
    {
        // 씬 진입 시 최초 설정
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

### 코루틴 & 비동기
- 코루틴 사용 ❌
- **UniTask 사용**

```csharp
public async UniTask FadeOutAsync()
{
    await UniTask.Delay(1000);
}
```

### Null 체크
- 외부 API 호출 결과만 null 체크
- 내부 로직에서는 null 체크 최소화

```csharp
// API 호출 - null 체크 O
var response = await api.GetDataAsync();
if (response == null) return;

// 내부 로직 - null 체크 X (설계로 보장)
player.TakeDamage(10);
```

### 기타
- `SerializeField private` 우선 사용
- `FindObjectOfType` / `GameObject.Find` 사용 최소화
- `Debug.Log`는 최종 커밋 전 제거 또는 조건부 사용

---

## 13. UI 아키텍처

### 소규모 UI
- **Controller 단일 구조**

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

### 대규모 UI - MVC 패턴

```
┌─────────────┐     외부 호출      ┌─────────────┐
│  External   │ ───────────────▶  │  Controller │
└─────────────┘                   └──────┬──────┘
                                         │ 명령
                         ┌───────────────┼───────────────┐
                         ▼               ▼               ▼
                   ┌─────────┐     ┌─────────┐     ┌─────────┐
                   │  Model  │     │  View   │     │ Service │
                   └─────────┘     └────┬────┘     └─────────┘
                         │              │
                         └──── 구독 ────┘
                           (delegate)
```

- **Controller**: 외부 호출 창구, 내부 명령 전달
- **Model**: 데이터 관리
- **View**: UI 표시, 델리게이트로 Model 구독
- **Service**: Controller 비대화 시 추가, M/V에 맞지 않는 로직 담당

```csharp
// Model
public class InventoryModel
{
    public event Action<List<Item>> OnItemsChanged;

    private List<Item> items = new();

    public void AddItem(Item item)
    {
        items.Add(item);
        OnItemsChanged?.Invoke(items);
    }
}

// View
public class InventoryView : MonoBehaviour
{
    public void Bind(InventoryModel model)
    {
        model.OnItemsChanged += UpdateDisplay;
    }

    private void UpdateDisplay(List<Item> items) { }
}

// Controller
public class InventoryController : MonoBehaviour
{
    private InventoryModel model;
    private InventoryView view;
    private InventoryService service; // 필요 시

    public void Initialize()
    {
        model = new InventoryModel();
        view.Bind(model);
    }

    // 외부 호출 창구
    public void AddItem(Item item)
    {
        model.AddItem(item);
    }
}

// Service (Controller 비대화 시 추가)
public class InventoryService
{
    // 정렬, 필터링, 복잡한 로직 등
    public List<Item> SortByRarity(List<Item> items) { }
}
```

---

## 14. 매직 넘버 / 매직 스트링 금지

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

---

## 15. Prefab / Asset 네이밍

| 분류 | 규칙 | 예시 |
|------|------|------|
| Prefab | `{기능}_{상태/타입}.prefab` | `Player_Idle.prefab`, `Enemy_Boss.prefab` |
| Sprite | `{카테고리}_{이름}.png` | `Icon_Coin.png`, `UI_Button_Primary.png` |
| Audio | `{타입}_{이름}.wav` | `SFX_Jump.wav`, `BGM_Main.mp3` |
| Material | `Mat_{이름}.mat` | `Mat_Player.mat`, `Mat_Ground.mat` |
| Animation | `{대상}@{동작}.anim` | `Player@Run.anim`, `Enemy@Attack.anim` |

---

## 16. 인터페이스 활용

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

---

## 17. 성능 규칙

### GC 최소화
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

### 박싱 방지
```csharp
// ❌ 박싱 발생
object value = 10;  // int → object
string.Format("{0}", 10);  // 박싱

// ✅ 제네릭 사용
void Process<T>(T value) where T : struct { }
$"Score: {score}"  // 보간 문자열
```

### 오브젝트 풀링
```csharp
// ❌ 반복 생성/파괴
Instantiate(bulletPrefab);
Destroy(bullet);

// ✅ 풀링 사용
var bullet = bulletPool.Get();
bulletPool.Release(bullet);
```

### LINQ 사용 금지
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

### GetComponent 캐싱
- `GetComponent<T>()`는 캐싱 필수

### Camera.main 사용 금지
- `Camera.main` 직접 사용 ❌
- **CameraManager를 통해 접근**

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

> **이유**: Camera.main은 내부에서 FindWithTag를 호출하여 매번 검색 비용 발생

---

## 18. 주석 스타일

```csharp
/// <summary>
/// XML 문서 주석은 public API에만 사용
/// </summary>
public void PublicMethod() { }

// 일반 주석은 "왜"를 설명 (무엇을 하는지는 코드로)
private void InternalMethod() { }
```

---

## 19. 네임스페이스

- **기본: 생략**
- **충돌 시에만 1단계 사용**

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

---

## 20. 자동 포맷 & 도구

- `.editorconfig`는 프로젝트 기준을 따른다
- VSCode 사용자는 다음 확장 설치 권장:
  - EditorConfig for VS Code
  - C# (Microsoft)

---

## 🔚 마무리 원칙

> 이 문서는 "강제 규칙"이 아니라 "팀의 합의 기준"이다.
> 리뷰 시 스타일보다 로직과 안정성을 우선한다.
