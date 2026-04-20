# Unity 프레임워크 포터블 아키텍처 가이드

> FacadeManager 기반 싱글톤 매니저 시스템, UI 3계층, Provider 패턴 Data Layer, FSM GameState, EventManager 이벤트 버스를 포함하는 Unity 프레임워크 아키텍처 가이드.
> 이 문서를 CLAUDE.md에서 참조하면, Claude Code가 동일한 아키텍처 패턴으로 코드를 생성할 수 있다.

---

## Part 1: 프레임워크 개요

### 1. 설계 원칙

| 원칙 | 설명 |
|------|------|
| **Singleton + Attribute DI** | `Singleton<T>` 베이스 + `[Singleton(typeof(T))]` 어트리뷰트로 매니저 간 의존성 선언. `InjectUtil.InjectSingleton(this)`로 주입. |
| **위상정렬 초기화** | `DependencyResolver`가 Kahn's Algorithm으로 `[Singleton]` 의존성을 분석하여 매니저 초기화 순서를 자동 결정. |
| **이벤트 기반 통신** | `EventManager`의 `PostNotification<TEnum>()` / `IListener<TEnum>.OnEvent()`로 매니저 간 간접 통신. |
| **Provider 추상화** | `DataManager`가 `IXxxDataProvider` 인터페이스 기반으로 데이터 소스(JSON/API/Mock) 교체 가능. |
| **UI 3계층** | Scene(Dict 캐싱) / Popup(Stack LIFO) / Sub(Queue 풀링) 3종 UI를 `UIManager`가 통합 관리. |

### 1-1. 시스템 개요도

```
┌────────────────────── Presentation Layer ──────────────────────┐
│   UIManager (Scene/Popup/Sub)   ·   LoadingManager            │
│   UILayoutManager (가로/세로 레이아웃 전환)                    │
├────────────────────── Game Logic Layer ────────────────────────┤
│   GameStateManager   ·   InputManager                         │
├────────────────────── Core Service Layer ──────────────────────┤
│   EventManager   ·   ErrorEventManager   ·   GameSceneManager │
│   CameraManager                                               │
├────────────────────── Data & Resource Layer ───────────────────┤
│   DataManager   ·   AddressableManager   ·   ObjectManager    │
│   APIManager    ·   SoundManager         ·   FactoryManager   │
├────────────────────── Platform Layer ─────────────────────────┤
│   PlatformManager (플랫폼 판별 · Provider 팩토리)             │
│   MessageManager (플랫폼 ↔ Unity Bridge)                      │
└───────────────────────────────────────────────────────────────┘
                              ▲
                       FacadeManager
                  (수집 → 정렬 → 초기화)
```

### 1-2. 매니저 상호작용 3가지 방식

| 방식 | 설명 | 예시 |
|------|------|------|
| `[Singleton]` 직접 참조 | Init 시 어트리뷰트 기반 주입, 동기 호출 | `GameStateManager → EventManager` |
| `EventManager` 이벤트 | `PostNotification()` → `OnEvent()` 간접 통신 | `GameStateManager ··→ UIManager (GameStateEvent)` |
| `AddressableManager` 리소스 | 프리팹/에셋 비동기 로드 요청 | `UIManager ══→ AddressableManager` |

---

### 2. 디렉토리 구조 템플릿

```
Assets/@Project/
├── 00.Scenes/          # Unity 씬 파일
├── 01.Scripts/
│   ├── Base/           # 상수 클래스 (UIKeys, APIKeys, SoundKeys 등)
│   │   └── Platform/   # PlatformType, IPlatformSetup, 플랫폼별 Setup
│   ├── Data/           # 데이터 모델, Provider 인터페이스 및 구현체
│   │   └── Provider/   # IXxxDataProvider, JsonXxxProvider, ApiXxxProvider
│   ├── Enum/           # GameStateType, SceneEvent, InputEvent 등 열거형
│   ├── Manager/
│   │   ├── Facade/     # FacadeManager
│   │   ├── Interface/  # IManager, IErrorListener 등 매니저 인터페이스
│   │   └── Manager/    # 싱글톤 매니저들 (partial class 포함)
│   │       ├── EventManager/
│   │       ├── ErrorEventManager/
│   │       ├── GameStateManager/
│   │       │   └── Interface/  # IGameState
│   │       ├── DataManager/    # DataManager.cs + partial 파일들
│   │       ├── APIManager/     # APIManager.cs + partial 파일들
│   │       └── ...
│   ├── Obj/Object/     # OBJ_Base 기반 게임 오브젝트
│   ├── Scene/          # 씬별 초기화 (Main.cs, Planet.cs) + Screen 클래스
│   ├── Singleton/      # Singleton<T> 제네릭 베이스
│   ├── UI/
│   │   ├── Base/       # UI_Base, UI_Scene, UI_Popup, UI_Sub, UIKeys, IView
│   │   ├── Component/  # 공용 UI 컴포넌트
│   │   ├── UI_Popup/   # 팝업 UI 구현체
│   │   ├── UI_Scene/   # 씬 UI 구현체
│   │   └── UI_Sub/     # 서브 UI 구현체
│   └── Util/           # InjectUtil, LogUtil 등 유틸리티
│       └── Inject/     # SingletonAttribute, FindComponentsAttribute, DependencyResolver, InjectUtil
├── 02.Prefabs/
├── 03.Sprites/
├── 04.Materials/
├── 05.Animations/
├── 06.Data/            # JSON 데이터 파일
├── 07.Sounds/
└── 08.Fonts/
```

---

## Part 2: 핵심 기반 클래스

### 3. Singleton\<T\>

MonoBehaviour 기반 제네릭 싱글톤. Lazy initialization + 자동 생성 + DontDestroyOnLoad.

```csharp
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
                SetupInstance();

            return instance;
        }
    }
    
    public static void SetupInstance()
    {
        instance = FindObjectOfType<T>();
        if (instance == null)
        {
            var go = new GameObject();
            go.name = typeof(T).Name;
            instance = go.AddComponent<T>();
            DontDestroyOnLoad(instance);
        }
    }
}
```

> **규칙**: 외부에서 `XxxManager.Instance`를 직접 접근하지 않는다. 반드시 `[Singleton(typeof(XxxManager))]` 어트리뷰트 + `InjectUtil.InjectSingleton(this)`로 주입한다.

---

### 4. IManager

모든 매니저가 구현하는 인터페이스. FacadeManager가 Init/Release를 호출한다.

```csharp
public interface IManager
{
    void Init();
    void Release();
}
```

---

### 5. SingletonAttribute + FindComponentsAttribute

#### SingletonAttribute

매니저 의존성을 필드 레벨에서 선언하는 어트리뷰트.

```csharp
using System;

[AttributeUsage(AttributeTargets.Field)]
public class SingletonAttribute : Attribute
{
    public Type type;
    public SingletonAttribute(Type type)
    {
        this.type = type;
    }
}
```

**사용 예:**
```csharp
[Singleton(typeof(EventManager))]
private EventManager eventManager;
```

#### FindComponentsAttribute

UI 계층 구조에서 자식 GameObject/Component를 자동 바인딩하는 어트리뷰트.

```csharp
using System;

[AttributeUsage(AttributeTargets.Field)]
public class FindComponentsAttribute : Attribute
{
    public string[] gameObjectNames { get; }
    public bool findChild;

    public FindComponentsAttribute(params string[] gameObjectNames)
    {
        this.gameObjectNames = gameObjectNames;
    }

    /// <param name="findChild">현재 스크립트를 기준으로 하위에서 바인딩 여부</param>
    /// <param name="gameObjectNames">바인딩할 게임오브젝트 명</param>
    public FindComponentsAttribute(bool findChild = false, params string[] gameObjectNames)
    {
        this.gameObjectNames = gameObjectNames;
        this.findChild = findChild;
    }
}
```

**사용 예:**
```csharp
// 단일 컴포넌트
[FindComponents("TitleText")]
private TextMeshProUGUI titleText;

// 배열
[FindComponents("Slot_1", "Slot_2", "Slot_3")]
private Image[] slotImages;

// 자식 기준 탐색 (같은 프리팹 내 동일 이름 방지)
[FindComponents(true, "ItemIcon")]
private Image itemIcon;
```

> **규칙**: `FindComponents`는 `Component` 타입 필드에만 적용 가능하다. `GameObject` 타입에는 적용 불가.

---

### 6. InjectUtil

리플렉션 기반 의존성 주입 유틸리티.

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public static class InjectUtil
{
    private static readonly string BindRootTag = "BindRoot";
    private static readonly Dictionary<Type, UnityEngine.Object> singletonCache = new Dictionary<Type, UnityEngine.Object>();

    public static void InjectComponents(object o)
    {
        Type type = o.GetType();
        MonoBehaviour script = o as MonoBehaviour;
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var attribute = (FindComponentsAttribute)field.GetCustomAttribute(typeof(FindComponentsAttribute));
            if (attribute == null)
                continue;
            
            if (field.FieldType.IsArray)
                InjectArrayField(field, attribute, script);
            else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                InjectListField(field, attribute, script);
            else
                InjectSingleComponent(field, attribute, script);
        }
    }

    private static void InjectArrayField(FieldInfo field, FindComponentsAttribute attribute, MonoBehaviour script)
    {
        Type elementType = field.FieldType.GetElementType();
        List<Component> componentsList = GetComponentsFromGameObjects(attribute, elementType, script);
        Array componentArray = Array.CreateInstance(elementType, componentsList.Count);
        for (int i = 0; i < componentsList.Count; i++)
        {
            componentArray.SetValue(componentsList[i], i);
        }
        field.SetValue(script, componentArray);
    }

    private static void InjectListField(FieldInfo field, FindComponentsAttribute attribute, MonoBehaviour script)
    {
        Type elementType = field.FieldType.GetGenericArguments()[0];
        IList componentsList = (IList)Activator.CreateInstance(field.FieldType);
        foreach (var component in GetComponentsFromGameObjects(attribute, elementType, script))
        {
            componentsList.Add(component);
        }
        field.SetValue(script, componentsList);
    }

    private static void InjectSingleComponent(FieldInfo field, FindComponentsAttribute attribute, MonoBehaviour script)
    {
        Transform tr;
        if (attribute.findChild)
            tr = FindChild(attribute.gameObjectNames[0], script.transform);
        else if (script.transform.CompareTag(BindRootTag))
            tr = FindChild(attribute.gameObjectNames[0], script.transform);
        else
            tr = FindInSameRoot(attribute.gameObjectNames[0], script.transform);
        if (tr == null)
            return;
        
        Component component = tr.GetComponent(field.FieldType);
        if (component != null)
            field.SetValue(script, component);
    }

    private static List<Component> GetComponentsFromGameObjects(FindComponentsAttribute attribute, Type componentType, MonoBehaviour script)
    {
        List<Component> componentsList = new List<Component>();
        foreach (string gameObjectName in attribute.gameObjectNames)
        {
            Transform tr;
            if (attribute.findChild)
                tr = FindChild(gameObjectName, script.transform);
            else if (script.transform.CompareTag(BindRootTag))
                tr = FindChild(gameObjectName, script.transform);
            else
                tr = FindInSameRoot(gameObjectName, script.transform);
            if (tr == null)
                continue;

            Component component = tr.GetComponent(componentType);
            if (component != null)
                componentsList.Add(component);
        }
        return componentsList;
    }

    private static Transform FindInSameRoot(string name, Transform startTr)
    {
        Transform findTr = null;
        while (findTr == null)
        {
            if (startTr.CompareTag(BindRootTag))
                findTr = startTr;
            startTr = startTr.parent;
        }
        return FindChild(name, findTr);
    }

    private static Transform FindChild(string name, Transform tr)
    {
        if (tr.name == name)
            return tr;
        for (int i = 0; i < tr.childCount; i++)
        {
            Transform findTr = FindChild(name, tr.GetChild(i));
            if (findTr != null)
                return findTr;
        }
        return null;
    }

    public static void InjectSingleton(object o)
    {
        Type type = o.GetType();
        FieldInfo[] fields = type.GetFields(BindingFlags.Public |
                                            BindingFlags.NonPublic |
                                            BindingFlags.Instance);
        foreach (var field in fields)
        {
            var attribute = (SingletonAttribute)field.GetCustomAttribute(typeof(SingletonAttribute));
            if (attribute == null)
                continue;

            Type singletonType = attribute.type;

            if (!singletonCache.TryGetValue(singletonType, out var singleton))
            {
                var property = singletonType.GetProperty("Instance",
                            BindingFlags.Static |
                            BindingFlags.Public |
                            BindingFlags.FlattenHierarchy |
                            BindingFlags.GetProperty);
                singleton = (UnityEngine.Object)property.GetValue(null, null);

                if (singleton != null)
                    singletonCache[singletonType] = singleton;
            }
            if (singleton != null)
                field.SetValue(o, singleton);
        }
    }
}
```

> **규칙**: `InjectUtil.InjectSingleton(this)`는 MonoBehaviour뿐 아니라 plain C# class에서도 사용 가능하다. Provider 등 일반 클래스의 생성자에서도 호출하여 매니저를 주입한다.

---

### 7. DependencyResolver

Kahn's Algorithm 기반 위상정렬로 매니저 초기화 순서를 자동 결정한다.

```csharp
using System;
using System.Collections.Generic;
using System.Reflection;

public static class DependencyResolver
{
    private static readonly string LogTag = "[DependencyResolver]";

    public static List<IManager> Sort(List<IManager> managers)
    {
        Dictionary<Type, List<Type>> graph = BuildDependencyGraph(managers);
        return TopologicalSort(managers, graph);
    }

    private static Dictionary<Type, List<Type>> BuildDependencyGraph(List<IManager> managers)
    {
        Dictionary<Type, List<Type>> graph = new Dictionary<Type, List<Type>>();

        for (int i = 0; i < managers.Count; i++)
        {
            IManager manager = managers[i];
            Type managerType = manager.GetType();
            List<Type> dependencies = new List<Type>();

            FieldInfo[] fields = managerType.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            for (int j = 0; j < fields.Length; j++)
            {
                FieldInfo field = fields[j];
                SingletonAttribute attr = (SingletonAttribute)field.GetCustomAttribute(typeof(SingletonAttribute));

                if (attr != null)
                    dependencies.Add(attr.type);
            }

            graph[managerType] = dependencies;
        }

        return graph;
    }

    private static List<IManager> TopologicalSort(List<IManager> managers, Dictionary<Type, List<Type>> graph)
    {
        Dictionary<Type, IManager> typeToManager = new Dictionary<Type, IManager>();
        Dictionary<Type, int> inDegree = new Dictionary<Type, int>();

        for (int i = 0; i < managers.Count; i++)
        {
            IManager manager = managers[i];
            Type type = manager.GetType();
            typeToManager[type] = manager;
            inDegree[type] = 0;
        }

        for (int i = 0; i < managers.Count; i++)
        {
            Type managerType = managers[i].GetType();

            if (graph.TryGetValue(managerType, out List<Type> dependencies))
            {
                for (int j = 0; j < dependencies.Count; j++)
                {
                    Type depType = dependencies[j];
                    if (inDegree.ContainsKey(depType))
                        inDegree[managerType]++;
                }
            }
        }

        Queue<Type> queue = new Queue<Type>();
        List<Type> managerTypes = new List<Type>(inDegree.Keys);

        for (int i = 0; i < managerTypes.Count; i++)
        {
            Type type = managerTypes[i];
            if (inDegree[type] == 0)
                queue.Enqueue(type);
        }

        List<IManager> sorted = new List<IManager>();

        while (queue.Count > 0)
        {
            Type current = queue.Dequeue();
            sorted.Add(typeToManager[current]);

            for (int i = 0; i < managers.Count; i++)
            {
                Type otherType = managers[i].GetType();

                if (graph.TryGetValue(otherType, out List<Type> dependencies))
                {
                    bool dependsOnCurrent = false;
                    for (int j = 0; j < dependencies.Count; j++)
                    {
                        if (dependencies[j] == current)
                        {
                            dependsOnCurrent = true;
                            break;
                        }
                    }

                    if (dependsOnCurrent)
                    {
                        inDegree[otherType]--;
                        if (inDegree[otherType] == 0)
                            queue.Enqueue(otherType);
                    }
                }
            }
        }

        if (sorted.Count != managers.Count)
        {
            LogUtil.LogError($"{LogTag} 순환 의존성이 감지되었습니다! 일부 매니저가 정렬되지 않았습니다.");

            for (int i = 0; i < managers.Count; i++)
            {
                IManager manager = managers[i];
                bool found = false;
                for (int j = 0; j < sorted.Count; j++)
                {
                    if (sorted[j] == manager)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    sorted.Add(manager);
            }
        }

        return sorted;
    }
}
```

---

### 8. FacadeManager

모든 매니저를 수집 → 의존성 정렬 → 순서대로 초기화 → 역순 해제.

```csharp
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class FacadeManager : MonoBehaviour
{
    private static readonly string LogTag = "[FacadeManager]";
    private static bool isInitialized;
    private List<IManager> managers;

    private void Awake()
    {
        if (isInitialized)
        {
            Destroy(gameObject);
            return;
        }
        isInitialized = true;

        CollectManagers();
        SortByDependency();
        InitAll();
    }

    private void CollectManagers()
    {
        managers = new List<IManager>
        {
            // 여기에 프로젝트의 모든 매니저를 등록한다
            EventManager.Instance,
            ErrorEventManager.Instance,
            AddressableManager.Instance,
            PlatformManager.Instance,
            // ... 프로젝트에 맞게 추가
        };
    }

    private void SortByDependency()
    {
        managers = DependencyResolver.Sort(managers);
        LogInitOrder();
    }

    private void InitAll()
    {
        for (int i = 0; i < managers.Count; i++)
        {
            managers[i].Init();
        }
    }

    private void OnApplicationQuit()
    {
        ReleaseAll();
    }

    private void ReleaseAll()
    {
        for (int i = managers.Count - 1; i >= 0; i--)
        {
            managers[i].Release();
        }
    }

    private void LogInitOrder()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"{LogTag} Init Order: ");

        for (int i = 0; i < managers.Count; i++)
        {
            sb.Append(managers[i].GetType().Name);
            if (i < managers.Count - 1)
                sb.Append(" → ");
        }

        LogUtil.Log(sb.ToString());
    }
}
```

---

### 8-1. 초기화 / 해제 시퀀스

```
  Unity          FacadeManager         DependencyResolver      IManager[]
    │                  │                       │                    │
    │  ═══ 초기화 (Awake) ═════════════════════════════════════════ │
    ├── Awake() ─────>│                       │                    │
    │                  ├── CollectManagers()   │                    │
    │                  ├── Sort(managers) ────>│                    │
    │                  │                       ├─ BuildDependencyGraph()
    │                  │                       ├─ TopologicalSort()
    │                  │<── sorted List ──────┤                    │
    │                  │  [loop i = 0 → count-1]                   │
    │                  ├── managers[i].Init() ─────────────────────>│
    │                  │                       │  InjectUtil.       │
    │                  │                       │  InjectSingleton() │
    │                  │                       │                    │
    │  ═══ 해제 (OnApplicationQuit) ═══════════════════════════════ │
    │                  ├── ReleaseAll()        │                    │
    │                  │  [loop i = count-1 → 0  역순]             │
    │                  ├── managers[i].Release() ──────────────────>│
```

---

## Part 3: 매니저 시스템 가이드

### 9. 매니저 생성 템플릿

새 매니저를 추가할 때 따라야 할 코드 템플릿:

```csharp
using System.Collections.Generic;
using UnityEngine;

public class NewManager : Singleton<NewManager>, IManager
{
    private static readonly string LogTag = "[NewManager]";

    // 1. 의존성 선언 ([Singleton] 어트리뷰트)
    [Singleton(typeof(EventManager))]
    private EventManager eventManager;

    [Singleton(typeof(AddressableManager))]
    private AddressableManager addressableManager;

    // 2. private 필드
    private Dictionary<string, object> cache;

    // 3. 프로퍼티
    public bool IsReady { get; private set; }

    // 4. Init (FacadeManager에서 호출)
    public void Init()
    {
        InjectUtil.InjectSingleton(this);
        cache = new Dictionary<string, object>();
        IsReady = true;
    }

    // 5. public 메서드
    public void DoSomething()
    {
        // 구현
    }

    // 6. private 메서드
    private void InternalWork()
    {
        // 구현
    }

    // 7. Release (FacadeManager에서 역순 호출)
    public void Release()
    {
        cache.Clear();
        IsReady = false;
    }
}
```

---

### 10. Partial Class 분할 규칙

매니저가 커지면 도메인별로 partial class로 분할한다.

**Base 파일 (매니저명.cs):**
```csharp
public partial class DataManager : Singleton<DataManager>, IManager
{
    [Singleton(typeof(AddressableManager))]
    private AddressableManager addressableManager;

    public void Init()
    {
        InjectUtil.InjectSingleton(this);
        InitInventory();
        InitQuest();
    }

    public void Release()
    {
        ReleaseInventory();
        ReleaseQuest();
    }
}
```

**Partial 파일 (매니저명.도메인.cs):**
```csharp
public partial class DataManager
{
    private IInventoryDataProvider inventoryProvider;
    private Dictionary<int, InventoryItem> inventoryCache;

    partial void InitInventory()
    {
        inventoryProvider = new JsonInventoryDataProvider();
        inventoryCache = new Dictionary<int, InventoryItem>();
    }

    public void SetInventoryProvider(IInventoryDataProvider provider)
    {
        inventoryProvider = provider;
        inventoryCache.Clear();
    }

    partial void ReleaseInventory()
    {
        inventoryCache.Clear();
    }
}
```

**명명 규칙:**
- `DataManager.cs` (Base)
- `DataManager.Inventory.cs` (도메인별 partial)
- `DataManager.Quest.cs`
- `DataManager.Achievement.cs`

---

### 11. 매니저 등록 방법

새 매니저 추가 시 `FacadeManager.CollectManagers()`에 등록:

```csharp
private void CollectManagers()
{
    managers = new List<IManager>
    {
        // ... 기존 매니저들 ...
        NewManager.Instance,  // 추가
    };
}
```

> DependencyResolver가 `[Singleton]` 어트리뷰트를 분석하여 자동으로 올바른 초기화 순서를 결정하므로, 리스트 내 순서는 중요하지 않다.

---

## Part 4: 이벤트 시스템

### 12. EventManager

제네릭 Enum 기반 타입-안전 이벤트 버스.

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : Singleton<EventManager>, IManager
{
    private static readonly string LogTag = "[EventManager]";

    private Dictionary<Type, object> listenerTables;
    private Dictionary<Type, object> queueTables;

    public void Init()
    {
        listenerTables = new Dictionary<Type, object>();
        queueTables = new Dictionary<Type, object>();
    }

    #region Listener

    public void AddListener<TEnum>(IListener<TEnum> listener) where TEnum : Enum
    {
        if (listener == null) return;
        List<IListener<TEnum>> listeners = GetListeners<TEnum>();
        if (!listeners.Contains(listener))
            listeners.Add(listener);
    }

    public void RemoveListener<TEnum>(IListener<TEnum> listener) where TEnum : Enum
    {
        if (listener == null) return;
        List<IListener<TEnum>> listeners = GetListeners<TEnum>();
        listeners.Remove(listener);
    }

    public void RemoveAllListeners<TEnum>() where TEnum : Enum
    {
        Type type = typeof(TEnum);
        if (listenerTables.ContainsKey(type))
            listenerTables.Remove(type);
    }

    public void RemoveAllListeners()
    {
        listenerTables.Clear();
    }

    public void RemoveNullListeners<TEnum>() where TEnum : Enum
    {
        List<IListener<TEnum>> listeners = GetListeners<TEnum>();
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            if (listeners[i] == null)
                listeners.RemoveAt(i);
        }
    }

    private List<IListener<TEnum>> GetListeners<TEnum>() where TEnum : Enum
    {
        Type type = typeof(TEnum);
        if (!listenerTables.TryGetValue(type, out object table))
        {
            table = new List<IListener<TEnum>>();
            listenerTables[type] = table;
        }
        return (List<IListener<TEnum>>)table;
    }

    #endregion

    #region Notification

    public void PostNotification<TEnum>(TEnum eventType, object param = null) where TEnum : Enum
    {
        List<IListener<TEnum>> listeners = GetListeners<TEnum>();

        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            if (listeners[i] == null) continue;
            if (listeners[i] is MonoBehaviour mb && !mb.gameObject.activeInHierarchy)
                continue;

            try
            {
                listeners[i].OnEvent(eventType, param);
            }
            catch (Exception e)
            {
                LogUtil.LogError($"{LogTag} 이벤트 처리 오류 {typeof(TEnum).Name}.{eventType}: {e.Message}");
            }
        }
    }

    #endregion

    #region Queue

    public void Enqueue<TEnum>(TEnum eventType, object param = null) where TEnum : Enum
    {
        Queue<(TEnum, object)> queue = GetQueue<TEnum>();
        queue.Enqueue((eventType, param));
    }

    public void ProcessQueue<TEnum>() where TEnum : Enum
    {
        Queue<(TEnum, object)> queue = GetQueue<TEnum>();
        while (queue.Count > 0)
        {
            (TEnum eventType, object param) = queue.Dequeue();
            PostNotification(eventType, param);
        }
    }

    public void ClearQueue<TEnum>() where TEnum : Enum
    {
        Queue<(TEnum, object)> queue = GetQueue<TEnum>();
        queue.Clear();
    }

    private Queue<(TEnum, object)> GetQueue<TEnum>() where TEnum : Enum
    {
        Type type = typeof(TEnum);
        if (!queueTables.TryGetValue(type, out object table))
        {
            table = new Queue<(TEnum, object)>();
            queueTables[type] = table;
        }
        return (Queue<(TEnum, object)>)table;
    }

    #endregion

    public void Release()
    {
        listenerTables.Clear();
        queueTables.Clear();
    }
}
```

---

### 13. IListener\<TEnum\>

이벤트 리스너 인터페이스:

```csharp
using System;

public interface IListener<TEventType> where TEventType : Enum
{
    void OnEvent(TEventType eventType, object param = null);
}
```

---

### 14. 이벤트 발행/구독 패턴

#### 이벤트 Enum 정의

```csharp
public enum GameStateEvent
{
    StateEntered,
    StateExited,
    StatePaused,
    StateResumed
}

public enum SceneEvent
{
    LoadStarted,
    LoadCompleted,
    TransitionProgressUpdated,
    SceneReady
}

public enum InputEvent
{
    Tap,
    LongPressStart, LongPressing, LongPressEnd,
    DragStart, Dragging, HorizontalDragging, VerticalDragging, DragEnd,
    PinchStart, Pinching, PinchEnd
}
```

#### 구독 예시 (MonoBehaviour)

```csharp
public class InventoryUI : UI_Scene, IListener<GameStateEvent>
{
    [Singleton(typeof(EventManager))]
    private EventManager eventManager;

    public override void Init()
    {
        InjectUtil.InjectSingleton(this);
        eventManager.AddListener<GameStateEvent>(this);
    }

    public void OnEvent(GameStateEvent eventType, object param = null)
    {
        if (eventType == GameStateEvent.StateEntered)
        {
            GameStateType state = (GameStateType)param;
            // 상태 진입 처리
        }
    }

    public override void Release()
    {
        eventManager.RemoveListener<GameStateEvent>(this);
        base.Release();
    }
}
```

#### 발행 예시

```csharp
eventManager.PostNotification(GameStateEvent.StateEntered, GameStateType.Dashboard);
```

---

### 15. ErrorEventManager

API 에러 코드 기반 이벤트 시스템.

```csharp
using System;

[Serializable]
public struct ApiErrorResponse
{
    public string Timestamp;
    public string Message;
    public int ErrorCode;
}

public interface IErrorListener
{
    void OnErrorEvent(ApiErrorResponse error);
}
```

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

public class ErrorEventManager : Singleton<ErrorEventManager>, IManager
{
    private static readonly string LogTag = "[ErrorEventManager]";
    private Dictionary<int, List<IErrorListener>> listenerTable;

    public void Init()
    {
        listenerTable = new Dictionary<int, List<IErrorListener>>();
    }

    public void AddListener(int errorCode, IErrorListener listener)
    {
        if (listener == null) return;
        if (!listenerTable.TryGetValue(errorCode, out List<IErrorListener> listeners))
        {
            listeners = new List<IErrorListener>();
            listenerTable[errorCode] = listeners;
        }
        if (!listeners.Contains(listener))
            listeners.Add(listener);
    }

    public void RemoveListener(int errorCode, IErrorListener listener)
    {
        if (listener == null) return;
        if (listenerTable.TryGetValue(errorCode, out List<IErrorListener> listeners))
            listeners.Remove(listener);
    }

    public void PostNotification(ApiErrorResponse error)
    {
        if (error.ErrorCode == 0) return;
        if (!listenerTable.TryGetValue(error.ErrorCode, out List<IErrorListener> listeners))
            return;

        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            if (listeners[i] == null) continue;
            if (listeners[i] is MonoBehaviour mb && !mb.gameObject.activeInHierarchy)
                continue;

            try
            {
                listeners[i].OnErrorEvent(error);
            }
            catch (Exception e)
            {
                LogUtil.LogError($"{LogTag} 에러 이벤트 처리 오류 [{error.ErrorCode}]: {e.Message}");
            }
        }
    }

    public void Release()
    {
        listenerTable.Clear();
    }
}
```

---

## Part 5: GameState FSM

### 16. IGameState

```csharp
public interface IGameState
{
    GameStateType StateType { get; }
    void OnStateEnter();
    void OnStateUpdate();
    void OnStateExit();
    void OnStatePause();
    void OnStateResume();
}
```

### 17. GameStateManager

```csharp
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : Singleton<GameStateManager>, IManager
{
    private static readonly string LogTag = "[GameStateManager]";

    [Singleton(typeof(EventManager))]
    private EventManager eventManager;

    private Dictionary<GameStateType, IGameState> states;
    private IGameState currentState;
    private IGameState previousState;

    public IGameState CurrentState => currentState;
    public GameStateType CurrentStateType => currentState?.StateType ?? GameStateType.None;
    public GameStateType PreviousStateType => previousState?.StateType ?? GameStateType.None;

    public void Init()
    {
        states = new();
        InjectUtil.InjectSingleton(this);
    }

    public void RegisterState(IGameState state)
    {
        if (state == null) return;
        states[state.StateType] = state;
    }

    public void ChangeState(GameStateType stateType)
    {
        if (!states.TryGetValue(stateType, out IGameState nextState))
        {
            LogUtil.LogError($"{LogTag} 상태를 찾을 수 없음: {stateType}");
            return;
        }

        if (currentState != null && currentState.StateType == stateType)
            return;

        if (currentState != null)
        {
            currentState.OnStateExit();
            eventManager.PostNotification(GameStateEvent.StateExited, currentState.StateType);
        }

        previousState = currentState;
        currentState = nextState;

        currentState.OnStateEnter();
        eventManager.PostNotification(GameStateEvent.StateEntered, currentState.StateType);
    }

    public void UpdateCurrentState()
    {
        currentState?.OnStateUpdate();
    }

    public void PauseCurrentState()
    {
        if (currentState != null)
        {
            currentState.OnStatePause();
            eventManager.PostNotification(GameStateEvent.StatePaused, currentState.StateType);
        }
    }

    public void ResumeCurrentState()
    {
        if (currentState != null)
        {
            currentState.OnStateResume();
            eventManager.PostNotification(GameStateEvent.StateResumed, currentState.StateType);
        }
    }

    public void ReturnToPreviousState()
    {
        if (previousState != null)
            ChangeState(previousState.StateType);
    }

    public bool HasState(GameStateType stateType)
    {
        return states.ContainsKey(stateType);
    }

    public T GetState<T>(GameStateType stateType) where T : class, IGameState
    {
        if (states.TryGetValue(stateType, out IGameState state))
            return state as T;
        return null;
    }

    public void ClearAllStates()
    {
        if (currentState != null)
        {
            currentState.OnStateExit();
            eventManager.PostNotification(GameStateEvent.StateExited, currentState.StateType);
        }
        currentState = null;
        previousState = null;
        states.Clear();
    }

    public void Release()
    {
        currentState = null;
        previousState = null;
        states.Clear();
    }
}
```

---

### 18. 상태 구현 템플릿

```csharp
public class LobbyState : IGameState
{
    [Singleton(typeof(UIManager))]
    private UIManager uiManager;

    [Singleton(typeof(EventManager))]
    private EventManager eventManager;

    public GameStateType StateType => GameStateType.Lobby;

    public LobbyState()
    {
        InjectUtil.InjectSingleton(this);
    }

    public void OnStateEnter()
    {
        uiManager.ShowSceneUI<UI_LobbyBG>(UIKeys.Scene.UI_LobbyBG);
    }

    public void OnStateUpdate() { }

    public void OnStateExit()
    {
        uiManager.CloseSceneUI(UIKeys.Scene.UI_LobbyBG);
    }

    public void OnStatePause() { }
    public void OnStateResume() { }
}
```

### 19. 씬별 상태 등록 패턴

각 씬의 초기화 스크립트에서 해당 씬에서 사용할 상태를 등록한다:

```csharp
public class Main : MonoBehaviour
{
    [Singleton(typeof(GameStateManager))]
    private GameStateManager gameStateManager;

    private void Start()
    {
        InjectUtil.InjectSingleton(this);
        RegisterStates();
    }

    private void RegisterStates()
    {
        gameStateManager.RegisterState(new LobbyState());
        gameStateManager.RegisterState(new DashboardState());
        gameStateManager.RegisterState(new InventoryState());
    }
}
```

---

## Part 6: UI 3계층 시스템

### 20. UI_Base

모든 UI의 추상 기반 클래스:

```csharp
using UnityEngine;

public abstract class UI_Base : MonoBehaviour
{
    public string UIKey { get; private set; }
    public int SortOrder { get; private set; }

    public abstract void Init();

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
    }

    public abstract void Release();

    public void SetUIKey(string key) => UIKey = key;

    public void SetSortOrder(int order)
    {
        SortOrder = order;
        var canvas = GetComponent<Canvas>();
        if (canvas != null)
            canvas.sortingOrder = order;
    }
}
```

---

### 21. UI_Scene / UI_Popup / UI_Sub

#### UI_Scene - 씬에 고정되는 UI

```csharp
public abstract class UI_Scene : UI_Base
{
    public override void Release()
    {
        UIManager.Instance.CloseSubUIs(this);
    }
}
```

#### UI_Popup - 스택으로 관리되는 팝업 UI

```csharp
public abstract class UI_Popup : UI_Base
{
    public override void Release()
    {
        UIManager.Instance.CloseSubUIs(this);
    }
}
```

#### UI_Sub - 오브젝트 풀링되는 서브 UI

```csharp
public abstract class UI_Sub : UI_Base
{
    public UI_Base OwnerUI { get; private set; }

    public void SetOwnerUI(UI_Base owner) => OwnerUI = owner;

    public virtual void OnSpawn() { }

    public virtual void OnDespawn() { }
}
```

---

### 22. UIManager

Scene(Dictionary 캐싱) / Popup(Stack LIFO) / Sub(Queue 풀링) 3종 UI를 통합 관리:

```csharp
using System.Collections.Generic;
using UnityEngine;

public partial class UIManager : Singleton<UIManager>, IManager
{
    private static readonly string LogTag = "[UIManager]";
    private static readonly string SceneUIRootName = "SceneUI_Root";
    private static readonly string PopupUIRootName = "PopupUI_Root";
    private static readonly string SubUIPoolRootName = "SubUI_Pool";

    [Singleton(typeof(AddressableManager))]
    private AddressableManager addressableManager;

    [Singleton(typeof(EventManager))]
    private EventManager eventManager;

    private Dictionary<string, UI_Scene> sceneUICache;
    private Dictionary<string, UI_Popup> popupUICache;
    private Dictionary<string, Queue<UI_Sub>> subUIPools;
    private Dictionary<UI_Base, HashSet<UI_Sub>> ownerSubUIMap;
    private Stack<UI_Popup> popupStack;
    private HashSet<string> openPopupKeys;

    private static readonly int SceneUiBaseOrder = 0;
    private static readonly int PopupBaseOrder = 100;
    private static readonly int PopupOrderIncrement = 10;
    private static readonly int DefaultOrderOffset = 1;

    private int currentSceneUIOrder = SceneUiBaseOrder;
    private Transform sceneUIRoot;
    private Transform popupUIRoot;
    private Transform subUIPoolRoot;

    public void Init()
    {
        InjectUtil.InjectSingleton(this);

        sceneUICache = new Dictionary<string, UI_Scene>();
        popupUICache = new Dictionary<string, UI_Popup>();
        subUIPools = new Dictionary<string, Queue<UI_Sub>>();
        ownerSubUIMap = new Dictionary<UI_Base, HashSet<UI_Sub>>();
        popupStack = new Stack<UI_Popup>();
        openPopupKeys = new HashSet<string>();
    }

    public void SetSceneUIRoot(Transform root)
    {
        ReleaseSceneUIs();

        GameObject sceneRoot = new GameObject(SceneUIRootName);
        sceneRoot.transform.SetParent(root);
        sceneUIRoot = sceneRoot.transform;

        GameObject popupRoot = new GameObject(PopupUIRootName);
        popupRoot.transform.SetParent(root);
        popupUIRoot = popupRoot.transform;

        GameObject subUIPool = new GameObject(SubUIPoolRootName);
        subUIPool.transform.SetParent(root);
        subUIPoolRoot = subUIPool.transform;
    }

    public void ReleaseSceneUIs()
    {
        foreach (var kvp in sceneUICache)
        {
            if (kvp.Value != null) kvp.Value.Release();
        }
        foreach (var kvp in popupUICache)
        {
            if (kvp.Value != null) kvp.Value.Release();
        }

        sceneUICache.Clear();
        popupUICache.Clear();
        subUIPools.Clear();
        ownerSubUIMap.Clear();
        popupStack.Clear();
        openPopupKeys.Clear();
        currentSceneUIOrder = SceneUiBaseOrder;
    }

    #region Scene UI

    public T ShowSceneUI<T>(string key) where T : UI_Scene
    {
        return ShowSceneUI<T>(key, DefaultOrderOffset);
    }

    public T ShowSceneUI<T>(string key, int orderOffset) where T : UI_Scene
    {
        if (!sceneUICache.TryGetValue(key, out UI_Scene sceneUI))
        {
            currentSceneUIOrder += orderOffset;
            int sortOrder = currentSceneUIOrder;

            sceneUI = CreateUI<T>(key, sceneUIRoot, sortOrder);
            if (sceneUI == null) return null;

            sceneUICache[key] = sceneUI;
        }

        sceneUI.Open();
        return sceneUI as T;
    }

    public void CloseSceneUI(string key)
    {
        if (sceneUICache.TryGetValue(key, out UI_Scene sceneUI))
            sceneUI.Close();
    }

    public T GetSceneUI<T>(string key) where T : UI_Scene
    {
        if (sceneUICache.TryGetValue(key, out UI_Scene sceneUI))
            return sceneUI as T;
        return null;
    }

    public void ResetSceneUIOrder()
    {
        currentSceneUIOrder = SceneUiBaseOrder;
    }

    #endregion

    #region Popup UI

    public T ShowPopupUI<T>(string key) where T : UI_Popup
    {
        if (openPopupKeys.Contains(key))
        {
            if (popupUICache.TryGetValue(key, out UI_Popup existingPopup))
                return existingPopup as T;
        }

        int sortOrder = PopupBaseOrder + (popupStack.Count * PopupOrderIncrement);

        if (!popupUICache.TryGetValue(key, out UI_Popup popupUI))
        {
            popupUI = CreateUI<T>(key, popupUIRoot, sortOrder);
            if (popupUI == null) return null;
            popupUICache[key] = popupUI;
        }
        else
        {
            popupUI.SetSortOrder(sortOrder);
        }

        popupStack.Push(popupUI);
        openPopupKeys.Add(key);
        popupUI.Open();

        return popupUI as T;
    }

    public void CloseTopPopup()
    {
        if (popupStack.Count > 0)
        {
            UI_Popup topPopup = popupStack.Pop();
            openPopupKeys.Remove(topPopup.UIKey);
            topPopup.Close();
        }
    }

    public void CloseAllPopups()
    {
        while (popupStack.Count > 0)
        {
            UI_Popup popup = popupStack.Pop();
            popup.Close();
        }
        openPopupKeys.Clear();
    }

    public T GetPopupUI<T>(string key) where T : UI_Popup
    {
        if (popupUICache.TryGetValue(key, out UI_Popup popupUI))
            return popupUI as T;
        return null;
    }

    public bool IsPopupOpen(string key) => openPopupKeys.Contains(key);
    public int PopupCount => popupStack.Count;

    #endregion

    #region Sub UI (Pooling)

    public T ShowSubUI<T>(string key, Transform parent) where T : UI_Sub
    {
        UI_Sub subUI;

        if (subUIPools.TryGetValue(key, out var pool) && pool.Count > 0)
        {
            subUI = pool.Dequeue();
        }
        else
        {
            subUI = CreateSubUI<T>(key, parent);
            if (subUI == null) return null;
        }

        subUI.transform.SetParent(parent);

        UI_Base ownerUI = parent.GetComponentInParent<UI_Base>();
        if (ownerUI != null)
        {
            subUI.SetOwnerUI(ownerUI);
            if (!ownerSubUIMap.ContainsKey(ownerUI))
                ownerSubUIMap[ownerUI] = new HashSet<UI_Sub>();
            ownerSubUIMap[ownerUI].Add(subUI);
        }

        subUI.OnSpawn();
        subUI.Open();

        return subUI as T;
    }

    private T CreateSubUI<T>(string key, Transform parent) where T : UI_Sub
    {
        GameObject prefab = addressableManager.Load<GameObject>(key);
        GameObject instance = Instantiate(prefab, parent);
        instance.name = key;

        T ui = instance.GetOrAddComponent<T>();
        ui.SetUIKey(key);
        ui.Init();

        return ui;
    }

    public void CloseSubUI(UI_Sub subUI)
    {
        if (subUI == null) return;

        if (subUI.OwnerUI != null && ownerSubUIMap.TryGetValue(subUI.OwnerUI, out var set))
            set.Remove(subUI);

        subUI.OnDespawn();
        subUI.Release();
        subUI.Close();
        subUI.transform.SetParent(subUIPoolRoot);
        subUI.SetOwnerUI(null);

        if (!subUIPools.ContainsKey(subUI.UIKey))
            subUIPools[subUI.UIKey] = new Queue<UI_Sub>();
        subUIPools[subUI.UIKey].Enqueue(subUI);
    }

    public void CloseSubUIs(UI_Base ownerUI)
    {
        if (ownerUI == null) return;
        if (!ownerSubUIMap.TryGetValue(ownerUI, out var set)) return;

        List<UI_Sub> subUIList = new List<UI_Sub>(set);
        for (int i = 0; i < subUIList.Count; i++)
        {
            CloseSubUI(subUIList[i]);
        }
    }

    public int GetActiveSubUICount(string key)
    {
        int count = 0;
        foreach (var kvp in ownerSubUIMap)
        {
            foreach (var subUI in kvp.Value)
            {
                if (subUI.UIKey == key)
                    count++;
            }
        }
        return count;
    }

    #endregion

    #region Utility

    private T CreateUI<T>(string key, Transform parent, int sortingOrder) where T : UI_Base
    {
        GameObject prefab = addressableManager.Load<GameObject>(key);
        GameObject instance = Instantiate(prefab, parent);
        instance.name = key;

        T ui = instance.GetOrAddComponent<T>();
        if (ui == null)
        {
            Destroy(instance);
            return null;
        }

        SetupCanvas(instance, sortingOrder);
        ui.SetUIKey(key);
        ui.Init();
        instance.SetActive(false);

        return ui;
    }

    public bool HasSceneUI(string key) => sceneUICache.ContainsKey(key);
    public bool HasPopupUI(string key) => popupUICache.ContainsKey(key);

    private void SetupCanvas(GameObject go, int sortingOrder)
    {
        Canvas canvas = go.GetComponent<Canvas>();
        if (canvas == null) return;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
    }

    #endregion

    public void Release()
    {
        CloseAllPopups();

        foreach (var kvp in sceneUICache)
        {
            if (kvp.Value != null)
            {
                kvp.Value.Release();
                Destroy(kvp.Value.gameObject);
            }
        }
        foreach (var kvp in popupUICache)
        {
            if (kvp.Value != null)
            {
                kvp.Value.Release();
                Destroy(kvp.Value.gameObject);
            }
        }
        foreach (var kvp in ownerSubUIMap)
        {
            foreach (var subUI in kvp.Value)
            {
                if (subUI != null)
                {
                    subUI.Release();
                    Destroy(subUI.gameObject);
                }
            }
        }
        foreach (var kvp in subUIPools)
        {
            while (kvp.Value.Count > 0)
            {
                var subUI = kvp.Value.Dequeue();
                if (subUI != null)
                    Destroy(subUI.gameObject);
            }
        }

        sceneUICache.Clear();
        popupUICache.Clear();
        subUIPools.Clear();
        ownerSubUIMap.Clear();
        popupStack.Clear();
        openPopupKeys.Clear();
        currentSceneUIOrder = SceneUiBaseOrder;
    }
}
```

**UI 3계층 요약:**

| 레이어 | 클래스 | sortOrder | 관리 방식 | 특징 |
|--------|--------|-----------|-----------|------|
| Scene UI | `UI_Scene` | 0 ~ 99 | Dictionary 캐싱 | 고정 HUD, 생성 시 +1 자동 증가 |
| Popup UI | `UI_Popup` | 100+ | Stack(LIFO) + 중복 방지 | 팝업 순서 +10씩, `CloseTopPopup()` |
| Sub UI | `UI_Sub` | 부모 따름 | Queue 기반 오브젝트 풀링 | `OnSpawn()/OnDespawn()`, 부모 UI 자동 연동 |

---

### 23. UIKeys 상수 클래스

UI 프리팹 이름과 1:1 매칭되는 상수. switch/case 사용을 위해 예외적으로 const 사용.

```csharp
public static class UIKeys
{
    public static class Scene
    {
        public const string UI_Lobby = "UI_Lobby";
        public const string UI_HUD = "UI_HUD";
        public const string UI_Inventory = "UI_Inventory";
        // ... 프로젝트에 맞게 추가
    }

    public static class Popup
    {
        public const string UI_ConfirmPopup = "UI_ConfirmPopup";
        public const string UI_SettingsPopup = "UI_SettingsPopup";
        // ... 프로젝트에 맞게 추가
    }

    public static class Sub
    {
        public const string UI_ItemSlot = "UI_ItemSlot";
        public const string UI_ListItem = "UI_ListItem";
        // ... 프로젝트에 맞게 추가
    }
}
```

---

### 24. UI MVC 패턴

대규모 UI는 Controller/Model/View/Service로 분리한다:

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

```csharp
// Model
public class InventoryModel
{
    public event Action<List<ItemData>> OnItemsChanged;
    private List<ItemData> items = new();

    public void AddItem(ItemData item)
    {
        items.Add(item);
        OnItemsChanged?.Invoke(items);
    }
}

// View (IView 인터페이스 구현)
public interface IView
{
    public void Init();
    public void Release();
}

public class InventoryView : MonoBehaviour, IView
{
    public void Init() { }

    public void Bind(InventoryModel model)
    {
        model.OnItemsChanged += UpdateDisplay;
    }

    private void UpdateDisplay(List<ItemData> items) { }

    public void Release() { }
}

// Controller (UI_Scene 상속)
public class UI_Inventory : UI_Scene
{
    private InventoryModel model;
    private InventoryView view;

    public override void Init()
    {
        model = new InventoryModel();
        view = GetComponent<InventoryView>();
        view.Init();
        view.Bind(model);
    }

    public void AddItem(ItemData item)
    {
        model.AddItem(item);
    }

    public override void Release()
    {
        view.Release();
        base.Release();
    }
}
```

---

## Part 7: 데이터 레이어

### 25. DataManager Partial 구조

```csharp
// DataManager.cs (Base)
public partial class DataManager : Singleton<DataManager>, IManager
{
    [Singleton(typeof(AddressableManager))]
    private AddressableManager addressableManager;

    public void Init()
    {
        InjectUtil.InjectSingleton(this);
        InitInventory();
        InitQuest();
        // ... 각 partial의 Init 호출
    }

    public void Release()
    {
        ReleaseInventory();
        ReleaseQuest();
    }
}

// DataManager.Inventory.cs
public partial class DataManager
{
    private IInventoryDataProvider inventoryProvider;
    private Dictionary<int, InventoryItem> inventoryCache;

    partial void InitInventory()
    {
        inventoryProvider = new JsonInventoryDataProvider("InventoryData");
        inventoryCache = new Dictionary<int, InventoryItem>();
    }

    public async UniTask<List<InventoryItem>> GetInventoryAsync(long userId)
    {
        return await inventoryProvider.GetItemsAsync(userId);
    }

    public void SetInventoryProvider(IInventoryDataProvider provider)
    {
        inventoryProvider = provider;
        inventoryCache.Clear();
    }

    partial void ReleaseInventory()
    {
        inventoryCache.Clear();
    }
}
```

---

### 26. Provider 인터페이스 설계

```csharp
public interface IInventoryDataProvider
{
    UniTask<List<InventoryItem>> GetItemsAsync(long userId);
    UniTask<bool> SaveItemAsync(long userId, InventoryItem item);
    UniTask<bool> PurchaseItemAsync(long userId, string itemId);
}
```

---

### 27. Json Provider 템플릿

Addressable JSON 파일 기반 로컬 구현:

```csharp
public class JsonInventoryDataProvider : IInventoryDataProvider
{
    [Singleton(typeof(AddressableManager))]
    private AddressableManager addressableManager;

    private string addressableKey;
    private List<InventoryItem> cachedData;
    private Dictionary<long, List<InventoryItem>> localSaves = new();

    public JsonInventoryDataProvider(string key)
    {
        InjectUtil.InjectSingleton(this);
        addressableKey = key;
    }

    public UniTask<List<InventoryItem>> GetItemsAsync(long userId)
    {
        LoadJsonIfNeeded();
        return UniTask.FromResult(cachedData ?? new List<InventoryItem>());
    }

    public UniTask<bool> SaveItemAsync(long userId, InventoryItem item)
    {
        if (!localSaves.ContainsKey(userId))
            localSaves[userId] = new List<InventoryItem>();
        localSaves[userId].Add(item);
        return UniTask.FromResult(true);
    }

    public UniTask<bool> PurchaseItemAsync(long userId, string itemId)
    {
        return UniTask.FromResult(true);
    }

    private void LoadJsonIfNeeded()
    {
        if (cachedData != null) return;
        TextAsset textAsset = addressableManager.Load<TextAsset>(addressableKey);
        cachedData = JsonConvert.DeserializeObject<List<InventoryItem>>(textAsset.text);
    }

    public void ClearCache()
    {
        cachedData = null;
    }
}
```

---

### 28. Api Provider 템플릿

APIManager 연동 서버 구현:

```csharp
public class ApiInventoryDataProvider : IInventoryDataProvider
{
    [Singleton(typeof(APIManager))]
    private APIManager apiManager;

    public ApiInventoryDataProvider()
    {
        InjectUtil.InjectSingleton(this);
    }

    public async UniTask<List<InventoryItem>> GetItemsAsync(long userId)
    {
        string endpoint = APIKeys.Inventory.Items + $"?userId={userId}";
        var response = await apiManager.GetAsync<InventoryListResponse>(endpoint);
        return response?.items ?? new List<InventoryItem>();
    }

    public async UniTask<bool> SaveItemAsync(long userId, InventoryItem item)
    {
        string endpoint = APIKeys.Inventory.Save + $"?userId={userId}";
        var response = await apiManager.PutAsync<SaveResponse, InventoryItem>(endpoint, item);
        return response != null;
    }

    public async UniTask<bool> PurchaseItemAsync(long userId, string itemId)
    {
        string endpoint = APIKeys.Inventory.Purchase + $"?userId={userId}";
        var request = new { itemId = itemId };
        var response = await apiManager.PostAsync<PurchaseResponse, object>(endpoint, request);
        return response?.success ?? false;
    }
}
```

---

## Part 8: API 통신

### 29. APIManager 4-Partial 구조

```csharp
// APIManager.cs (Base)
public partial class APIManager : Singleton<APIManager>, IManager
{
    [Singleton(typeof(PlatformManager))]
    private PlatformManager platformManager;

    [Singleton(typeof(ErrorEventManager))]
    private ErrorEventManager errorEventManager;

    private string baseUrl;
    private Dictionary<string, string> defaultHeaders;

    public void Init()
    {
        InjectUtil.InjectSingleton(this);
        defaultHeaders = new Dictionary<string, string>();
        SetBaseUrl(platformManager.GetBaseUrl());
        SetTokenProvider(platformManager.CreateTokenProvider());
        SetRequestHandler(platformManager.CreateRequestHandler(this));
    }

    public void SetBaseUrl(string url) { baseUrl = url; }
    public void SetDefaultHeader(string key, string value) { defaultHeaders[key] = value; }
    public void RemoveDefaultHeader(string key) { defaultHeaders.Remove(key); }
    public void ClearDefaultHeaders() { defaultHeaders.Clear(); }

    public void Release()
    {
        ClearDefaultHeaders();
        ClearToken();
    }
}

// APIManager.Request.cs
public partial class APIManager
{
    private IRequestHandler requestHandler;

    public void SetRequestHandler(IRequestHandler handler) { requestHandler = handler; }

    public async UniTask<TResponse> GetAsync<TResponse>(string endpoint)
    {
        return await SendRequestAsync<TResponse, object>(endpoint, "GET", null);
    }

    public async UniTask<TResponse> PostAsync<TResponse, TRequest>(string endpoint, TRequest body)
    {
        return await SendRequestAsync<TResponse, TRequest>(endpoint, "POST", body);
    }

    public async UniTask<TResponse> PutAsync<TResponse, TRequest>(string endpoint, TRequest body)
    {
        return await SendRequestAsync<TResponse, TRequest>(endpoint, "PUT", body);
    }

    public async UniTask<TResponse> DeleteAsync<TResponse>(string endpoint)
    {
        return await SendRequestAsync<TResponse, object>(endpoint, "DELETE", null);
    }
}

// APIManager.Response.cs
public partial class APIManager
{
    public void NotifyApiError(string responseBody)
    {
        ApiErrorResponse error = JsonConvert.DeserializeObject<ApiErrorResponse>(responseBody);
        errorEventManager.PostNotification(error);
    }
}

// APIManager.Token.cs
public partial class APIManager
{
    private ITokenProvider tokenProvider;

    public bool HasToken => tokenProvider?.HasToken ?? false;

    public void SetTokenProvider(ITokenProvider provider) { tokenProvider = provider; }

    public async UniTask<string> RequestTokenAsync()
    {
        return await tokenProvider.RequestTokenAsync();
    }

    public async UniTask<bool> RefreshTokenAsync()
    {
        bool success = await tokenProvider.RefreshTokenAsync();
        if (success)
            SetDefaultHeader("Authorization", $"Bearer {tokenProvider.Token}");
        return success;
    }

    public void ClearToken() { tokenProvider?.ClearToken(); }
}
```

---

### 30. IRequestHandler / ITokenProvider

```csharp
// 플랫폼별 HTTP 요청 처리 추상화
public interface IRequestHandler
{
    UniTask<TResponse> SendAsync<TResponse>(
        UnityWebRequest request, string endpoint, string method, string jsonBody, bool isRetry);
    UniTask<bool> SendBinaryAsync(UnityWebRequest request);
}

// JWT 토큰 관리 추상화
public interface ITokenProvider
{
    bool HasToken { get; }
    string Token { get; }
    UniTask<string> RequestTokenAsync();
    UniTask<bool> RefreshTokenAsync();
    void ClearToken();
}
```

---

## Part 9: 플랫폼 메시징 레이어

### 32. 메시지 구조

```csharp
// 메시지 마커 인터페이스
public interface IU2PMessage { }

// 단방향 메시지
[Serializable]
public class U2PMessage<T> : IU2PMessage
{
    public string type;
    public T payload;
}

// 요청-응답 메시지
[Serializable]
public class U2PRequest<T> : U2PMessage<T>
{
    public string requestId;
}
```

---

### 33. 단방향 vs 요청-응답 패턴

**PlatformMessageManager 템플릿:**

```csharp
public partial class PlatformMessageManager : Singleton<PlatformMessageManager>, IManager
{
    private readonly Dictionary<string, UniTaskCompletionSource<string>> pendingRequests = new();

    public void Init()
    {
        InjectUtil.InjectSingleton(this);
    }

    public void Release()
    {
        pendingRequests.Clear();
    }
}

// Send Partial
public partial class PlatformMessageManager
{
    private static readonly object EmptyPayload = new { };

    // 단방향 전송
    public void Send(string type)
    {
        Send(new U2PMessage<object> { type = type, payload = EmptyPayload });
    }

    public void Send<T>(string type, T payload)
    {
        Send(new U2PMessage<T> { type = type, payload = payload });
    }

    private void Send(IU2PMessage message)
    {
        string json = JsonConvert.SerializeObject(message);
        // 플랫폼별 전송 구현
    }

    // 요청-응답 (async/await)
    public async UniTask<TResponse> SendAsync<TResponse, TPayload>(string type, TPayload payload)
    {
        string requestId = Guid.NewGuid().ToString();
        var tcs = new UniTaskCompletionSource<string>();
        pendingRequests[requestId] = tcs;

        var request = new U2PRequest<TPayload>
        {
            type = type,
            payload = payload,
            requestId = requestId
        };

        Send(request);

        string response = await tcs.Task;
        return JsonConvert.DeserializeObject<TResponse>(response);
    }
}

// Receive Partial
public partial class PlatformMessageManager
{
    private static readonly Dictionary<string, SomeEvent> messageToEventMap = new()
    {
        // { "message_type", SomeEvent.EventValue },
    };

    public void OnMessage(string data)
    {
        try
        {
            // 응답인지 확인
            var responseBase = JsonConvert.DeserializeObject<ResponseBase>(data);
            if (!string.IsNullOrEmpty(responseBase.requestId))
            {
                HandleResponse(responseBase.requestId, data);
                return;
            }

            // 메시지 타입 추출
            var baseMessage = JsonConvert.DeserializeObject<MessageBase>(data);
            string messageType = baseMessage.type;

            // 이벤트 매핑
            if (messageToEventMap.TryGetValue(messageType, out var eventValue))
            {
                // eventManager.PostNotification(eventValue);
                return;
            }
        }
        catch (Exception e)
        {
            LogUtil.LogError($"[PlatformMessageManager] 파싱 오류: {e.Message}");
        }
    }

    private void HandleResponse(string requestId, string data)
    {
        if (pendingRequests.TryGetValue(requestId, out var tcs))
        {
            tcs.TrySetResult(data);
            pendingRequests.Remove(requestId);
        }
    }
}
```

---

## Part 10: 보조 매니저 API 가이드

### AddressableManager

| API | 설명 |
|-----|------|
| `PreloadAsync<T>(string label)` | 라벨 기반 에셋 프리로드 |
| `Load<T>(string key)` | 캐시에서 에셋 로드 (프리로드 필수) |
| `TryLoad<T>(string key)` | 캐시에서 에셋 로드 (없으면 default) |
| `ReleaseLabel(string label)` | 라벨별 리소스 해제 |
| `ReleaseAll()` | 모든 프리로드 리소스 해제 |

### ObjectManager

| API | 설명 |
|-----|------|
| `Spawn<T>(string key, Vector3 pos, Quaternion rot, Transform parent)` | 오브젝트 스폰 |
| `Spawn<T>(string key, Transform parent)` | 오브젝트 스폰 (기본 위치) |
| `Despawn(OBJ_Base obj)` | 오브젝트 디스폰 (풀 반환) |
| `SetSceneRoot(Transform root)` | 씬 루트 설정 |
| `Clear()` | 활성 오브젝트 정리 |

### InputManager

| API | 설명 |
|-----|------|
| `SetHandler(IInputHandler handler)` | 핸들러 설정 (루프 시작) |
| `ClearHandler()` | 핸들러 제거 (루프 정지) |
| `InputEnabled` (get/set) | 입력 활성화 여부 |

**IInputHandler 인터페이스:**
```csharp
public interface IInputHandler
{
    void OnTap(InputData data);
    void OnDoubleTap(InputData data);
    void OnDragStart(InputData data);
    void OnDragging(InputData data);
    void OnDragEnd(InputData data);
    void OnLongPressStart(InputData data);
    void OnLongPressing(InputData data);
    void OnLongPressEnd(InputData data);
    void OnPinchStart(PinchData data);
    void OnPinching(PinchData data);
    void OnPinchEnd(PinchData data);
    void OnClear();
}
```

### CameraManager

| API | 설명 |
|-----|------|
| `MainCamera` (get) | 메인 카메라 참조 |
| `SetHandler(ICameraHandler handler)` | 카메라 핸들러 설정 |
| `ClearHandler()` | 핸들러 제거 |
| `SetCutBlend()` | 즉시 전환 블렌드 |
| `SetSmoothBlend()` | 부드러운 전환 복원 |

### SoundManager

| API | 설명 |
|-----|------|
| `PlayBGM(string name)` | BGM 재생 |
| `StopBGM()` | BGM 정지 |
| `PlaySFX(AudioClip clip)` | SFX 재생 |
| `SetSFXVolume(float volume)` | SFX 볼륨 설정 |

### GameSceneManager

| API | 설명 |
|-----|------|
| `TransitionToSceneAsync(SceneType)` | 씬 전환 |
| `LoadPreviousSceneAsync()` | 이전 씬으로 복귀 |
| `IsTransitioning` (get) | 전환 중 여부 |
| `CurrentScene` (get) | 현재 씬 |
| `ClearHistory()` | 씬 히스토리 초기화 |

### PlatformManager

| API | 설명 |
|-----|------|
| `GetBaseUrl()` | API 기본 URL |
| `CreateTokenProvider()` | 플랫폼별 토큰 프로바이더 생성 |
| `CreateRequestHandler(APIManager)` | 플랫폼별 요청 핸들러 생성 |

```csharp
public enum PlatformType
{
    Editor,
    WebGL,
    App,
}

public interface IPlatformSetup
{
    void Initialize();
    void Release();
}
```

### LoadingManager

| API | 설명 |
|-----|------|
| `PreloadAsync()` | 전환 효과 프리팹 프리로드 |
| `CloseCurrentTransition()` | 현재 전환 효과 닫기 |
| `Clear()` | 캐시 정리 |

### UILayoutManager

| API | 설명 |
|-----|------|
| `CurrentMode` (get) | 현재 레이아웃 모드 (Landscape/Portrait) |
| `GetReferenceResolution(LayoutMode)` | 레이아웃 모드별 기준 해상도 |

---

### 오브젝트 수명주기 다이어그램

```
┌── ObjectManager 풀링 ──────────────────────────────┐
│  [Prefab] ──최초 생성──> [Instantiate] ──> [Active] │
│                                             │  ▲    │
│                                   Despawn() │  │    │
│                                             ▼  │    │
│                                          [Pool]     │
│                                             Spawn() │
└─────────────────────────────────────────────────────┘

┌── UIManager Sub 풀링 ────────────────────────────────┐
│  [Prefab] ──최초 생성──> [Create] ──> [Pool]          │
│                                       │  ▲            │
│                          ShowSubUI()  │  │            │
│                                       ▼  │            │
│                                    [Active]           │
│                                       CloseSubUI()    │
└───────────────────────────────────────────────────────┘
```

---

## Part 11: 코딩 컨벤션

### 네이밍 규칙

| 대상 | 규칙 | 예시 |
|------|------|------|
| 클래스 / 구조체 / Enum | PascalCase | `PlayerController`, `GameState` |
| 상수 / static readonly | PascalCase | `private static readonly int MaxHP = 100;` |
| public / protected 멤버 | PascalCase | `public int MaxHP;`, `public void Move();` |
| 프로퍼티 | PascalCase | `public int Health { get; private set; }` |
| private 필드 | camelCase (`_`, `m_` 금지) | `private bool isGrounded;` |
| 이벤트 / 델리게이트 | On + 동사 | `public event Action OnPlayerDeath;` |
| 비동기 메서드 | Async 접미어 | `public async UniTask LoadDataAsync()` |
| UI 클래스 | `UI_` 접두어 | `UI_Inventory`, `UI_Settings` |

### 금지 사항

| 항목 | 이유 | 대안 |
|------|------|------|
| **LINQ** | GC 발생, 디버깅 어려움 | for/foreach 반복문으로 직접 구현 |
| **코루틴** | 일관성, UniTask 통일 | UniTask 사용 |
| **람다식** | 디버깅 시 브레이크포인트 해제 불가 | 명시적 메서드 (외부 라이브러리 호환 시만 예외 허용) |
| **Update() / FixedUpdate()** | 성능, 명시적 제어 | UniTask 기반 루프 또는 Initialize() |
| **Camera.main** | FindWithTag 호출 비용 | CameraManager 사용 |
| **매직 넘버/스트링** | 가독성, 유지보수 | 상수 클래스로 관리 |
| **const** (일반) | DLL 버전 관리 문제 | `static readonly` 권장 (UIKeys는 예외: switch/case용) |
| **Awake()/Start()** (일반 객체) | 프레임워크/씬 초기화에만 허용 | `Initialize()` 메서드 사용 |
| **foreach** (성능 민감 코드) | 가능하면 for 루프 사용 | `for (int i = 0; i < count; i++)` |

### UniTask 비동기 규칙

```csharp
// 비동기 메서드는 Async 접미어 필수
public async UniTask FadeOutAsync()
{
    await UniTask.Delay(1000);
}

// 코루틴 사용 금지
// ❌ IEnumerator FadeOut() { yield return ... }
```

### 멤버 정렬 순서

```csharp
public class Example : MonoBehaviour
{
    // 1. 상수 (static readonly 권장)
    // 2. static 필드
    // 3. public 필드
    // 4. [SerializeField] 필드
    // 5. private 필드
    // 6. 프로퍼티
    // 7. Unity 콜백 (Awake, Start) - 프레임워크/씬 초기화만
    // 8. Initialize 메서드
    // 9. public 메서드
    // 10. private 메서드
    // 11. Release 메서드
}
```

### 성능 규칙

```csharp
// GC 최소화 - 캐싱/재사용
private List<int> cachedList = new();
private StringBuilder sb = new();

// 박싱 방지 - 제네릭 사용
void Process<T>(T value) where T : struct { }

// GetComponent 캐싱 필수
private Canvas cachedCanvas;

// 오브젝트 풀링 사용
var obj = objectManager.Spawn<T>(key);
objectManager.Despawn(obj);
```

### 주석/네임스페이스/var/this 규칙

- **주석**: "왜"를 설명, "무엇"은 코드로 표현
- **네임스페이스**: 기본 생략, 충돌 시에만 1단계 사용
- **var**: 타입이 명확한 경우에만 허용 (`var count = 10;` ✅, `var data = GetData();` ❌)
- **this**: 필드와 매개변수 이름 충돌 시에만 사용
- **문자열 비교**: `==` 사용, `.Equals()` 지양
- **문자열 가드**: `!= null` 대신 `string.IsNullOrEmpty()` 사용

### UI 아키텍처 규칙

- 소규모 UI: Controller 단일 구조
- 대규모 UI: MVC 패턴 (Controller → Model + View + Service)
- 인터페이스: 외부 의존성(API, DB 등)은 인터페이스로 추상화

---

## Part 12: 디자인 패턴 요약표

| 패턴 | 적용 위치 | 설명 |
|------|----------|------|
| **Facade** | `FacadeManager` | 매니저들의 단일 진입점. 수집/정렬/초기화/해제 통합 |
| **Singleton** | `Singleton<T>` | MonoBehaviour 기반 제네릭 싱글톤 |
| **Observer** | `EventManager` + `IListener<TEnum>` | 제네릭 이벤트 버스 |
| **State** | `GameStateManager` + `IGameState` | FSM 상태 머신 |
| **Strategy** | `IInputHandler`, `ICameraHandler`, `IBGMProvider`, `IRequestHandler` | 런타임 핸들러 교체 |
| **Object Pooling** | `ObjectManager`, `UIManager(Sub)`, `SoundManager(SFX)` | Queue 기반 재사용 |
| **Provider (DI)** | `DataManager`, `APIManager(Token)`, `PlatformManager` | 인터페이스 기반 구현체 교체 |
| **Partial Class** | `DataManager`, `APIManager`, `GameSceneManager`, `MessageManager` 등 | 도메인별 파일 분할 |
| **Attribute DI** | `[Singleton(typeof(T))]` + `InjectUtil` + `DependencyResolver` | 리플렉션 기반 위상정렬 초기화 |
| **MVC** | 대규모 UI (Controller/Model/View/Service) | 관심사 분리 |

---

## Part 13: Claude Code 작업 지침

### 파일 생성 규칙

- **한 파일 = 한 클래스**, 파일명과 클래스명 동일
- MonoBehaviour는 PascalCase 클래스명 필수
- Partial class는 `클래스명.도메인.cs` 형태

### 매니저 추가 체크리스트

1. `Singleton<T>` 상속 + `IManager` 구현
2. `[Singleton(typeof(...))]` 어트리뷰트로 의존성 선언
3. `Init()`에서 `InjectUtil.InjectSingleton(this)` 호출 + 초기화
4. `Release()`에서 리소스 정리
5. `FacadeManager.CollectManagers()`에 `NewManager.Instance` 등록
6. ARCHITECTURE.md 의존성 그래프 업데이트

### UI 추가 체크리스트

1. `UIKeys` 상수 등록 (Scene/Popup/Sub 적절한 카테고리)
2. Addressable 프리팹 생성 (키 = UIKeys 상수와 동일)
3. `UI_Scene` / `UI_Popup` / `UI_Sub` 중 적절한 타입 상속
4. `Init()` + `Release()` 구현
5. `InjectUtil.InjectComponents(this)` 또는 `InjectUtil.InjectSingleton(this)` 호출

### 이벤트 추가 체크리스트

1. `Enum` 파일에 새 이벤트 타입 추가
2. 발행자: `eventManager.PostNotification(NewEvent.SomeAction, param)`
3. 구독자: `IListener<NewEvent>` 구현 + `eventManager.AddListener<NewEvent>(this)` + `Release`에서 `RemoveListener`

### Provider 추가 체크리스트

1. `IXxxDataProvider` 인터페이스 정의 (`Data/Provider/`)
2. `JsonXxxDataProvider` 구현 (로컬 테스트용)
3. `ApiXxxDataProvider` 구현 (서버 연동용)
4. `DataManager.Xxx.cs` partial 파일 생성
5. DataManager Base의 `Init()`/`Release()`에 호출 추가

### 금지 행위

| 금지 | 이유 | 올바른 방법 |
|------|------|------------|
| `XxxManager.Instance` 직접 접근 | 의존성 추적 불가 | `[Singleton(typeof(XxxManager))]` + `InjectUtil.InjectSingleton(this)` |
| `UI_Base` 등 공용 베이스 클래스 수정 | 다른 모든 UI에 영향 | 매니저/파생 클래스 메서드로 해결 |
| 순환 의존 | DependencyResolver 실패 | 이벤트 기반 간접 통신 사용 |
| `.Equals()` 문자열 비교 | 일관성 | `==` 사용 |
| `string != null` 가드 | 빈 문자열 미처리 | `string.IsNullOrEmpty()` 사용 |

---

## 부록: CLAUDE.md 참조 템플릿

다른 프로젝트의 CLAUDE.md에 아래 내용을 추가하여 이 가이드를 참조할 수 있다:

```markdown
## Architecture & Convention

이 프로젝트는 FacadeManager 기반 싱글톤 매니저 시스템을 사용한다.
상세 아키텍처 가이드: [UNITY_FRAMEWORK_PORTABLE_GUIDE.md](UNITY_FRAMEWORK_PORTABLE_GUIDE.md)

핵심 규칙:
- [Singleton(typeof(T))] + InjectUtil.InjectSingleton(this) 패턴 사용
- Instance 직접 접근 금지
- LINQ/코루틴/Lambda/Update() 사용 금지
- UniTask 사용 (비동기 메서드 Async 접미어)
- UI 3계층: UI_Scene / UI_Popup / UI_Sub
- Provider 패턴: IXxxDataProvider → Json/Api 구현체
```
