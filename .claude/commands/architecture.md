# Architecture Review - ARCHITECTURE.md 기반 변경사항 검토

`ARCHITECTURE.md` 기준으로 현재 브랜치의 변경사항이 아키텍처를 올바르게 따르는지 검토합니다.

## 사용법

- `/arch-review` - 현재 브랜치의 모든 변경된 .cs 파일 검토
- `/arch-review [파일경로]` - 특정 파일만 검토
- `/arch-review [폴더경로]` - 특정 폴더의 변경 파일만 검토

## 작업 절차

### 검토 대상: $ARGUMENTS

1. **아키텍처 문서 로드**
   - `ARCHITECTURE.md` 파일을 읽어 프로젝트 아키텍처 기준을 파악

2. **변경사항 수집**
   - **인자가 없는 경우**: `git diff dev...HEAD --name-only`로 현재 브랜치에서 dev 대비 변경된 파일 목록 확인
   - **인자가 있는 경우**: 해당 파일/폴더로 범위 제한
   - `.cs` 파일만 필터링
   - 검토할 파일이 없으면 알림 후 종료
   - 각 변경 파일의 전체 코드를 읽어서 검토

3. **아키텍처 적합성 검토** (아래 체크리스트 기준으로 각 파일 검토)

4. **결과 리포트 출력**

## 검토 체크리스트

### 1. 매니저 계층 및 의존성 (Section 0, 1, 2)
- 새로운 매니저가 `Singleton<T>`, `IManager`를 올바르게 상속/구현하는가
- `[Singleton(typeof(...))]` 의존성 어트리뷰트가 Tier 구조를 위반하지 않는가
  - Tier 0: 의존성 없음 (EventManager, AddressableManager, FactoryManager, CameraManager, InputManager, APIManager)
  - Tier 1: Tier 0에만 의존 (DataManager, ObjectManager, UIManager, LoadingManager, FlutterMessageManager, GameSceneManager, GameStateManager)
  - Tier 2: Tier 0~1에 의존 (SoundManager, HexGridManager)
- 순환 의존성이 발생하지 않는가

### 2. 매니저 상호작용 방식 (Section 2-1)
- 매니저 간 직접 참조는 `[Singleton]` 어트리뷰트를 통해서만 이루어지는가
- 간접 통신은 `EventManager`의 `PostNotification()`/`OnEvent()`를 사용하는가
- 리소스 로드는 `AddressableManager`를 통하는가
- 매니저를 직접 `new`하거나 `FindObjectOfType`으로 참조하지 않는가

### 3. 이벤트 시스템 사용 (Section 4)
- `IListener<TEnum>` 인터페이스를 올바르게 구현하는가
- 이벤트 Enum이 정의된 타입(`SceneEvent`, `GameStateEvent`, `FlutterEvent`, `InputEvent`, `HexGridEvent`) 내에서 사용되는가
- 새로운 이벤트가 필요하면 기존 Enum을 확장하거나 새 Enum을 올바르게 정의했는가
- `AddListener`/`RemoveListener` 쌍이 올바르게 관리되는가 (Init/Release 또는 Enter/Exit에서)

### 4. GameState 상태 머신 (Section 5, 5-1)
- 새 GameState가 `IGameState` 인터페이스를 올바르게 구현하는가
- 상태 전환 흐름이 아키텍처 문서의 흐름도를 따르는가
  - `None -> Dashboard -> Depth0 -> Depth1 -> Depth1_5 -> Depth2 -> Depth3`
- `OnStateEnter/Exit/Update/Pause/Resume` 생명주기를 올바르게 사용하는가
- 상태 전환 시 `ChangeState()`와 `ReturnToPreviousState()`를 적절히 사용하는가

### 5. UI 시스템 구조 (Section 6)
- UI 클래스가 올바른 베이스 클래스를 상속하는가 (`UI_Scene`, `UI_Popup`, `UI_Sub`)
- Scene UI / Popup UI / Sub UI 계층을 올바르게 사용하는가
- Sub UI가 `OnSpawn()`/`OnDespawn()` 콜백을 구현하는가
- `UIManager`의 `ShowSceneUI`/`ShowPopupUI`/`ShowSubUI`를 올바르게 사용하는가

### 6. 오브젝트 수명주기 (Section 6-1)
- `ObjectManager`의 `Spawn/Despawn` 패턴을 따르는가
- 직접 `Instantiate`/`Destroy` 대신 풀링 시스템을 사용하는가
- 풀링 대상 오브젝트가 올바르게 반환(Despawn)되는가

### 7. 데이터 레이어 (Section 7)
- Provider 패턴(`IKnowledgeGraphProvider`, `IAvatarDataProvider`, `IFriendDataProvider` 등)을 올바르게 사용하는가
- 데이터 접근이 `DataManager`를 통하는가 (직접 Provider 접근 금지)
- API 호출이 `APIManager`를 통하는가
- 캐싱 전략을 따르는가

### 8. 입력 시스템 (Section 8)
- 입력 처리가 `IInputHandler` (Strategy 패턴)를 통하는가
- 각 GameState에 맞는 InputHandler가 설정되는가
- `InputData`/`PinchData` 구조체를 올바르게 사용하는가
- `IInputEventReceiver`를 통한 이벤트 브로드캐스트를 올바르게 사용하는가

### 9. 디자인 패턴 준수 (Section 10)
- **Facade**: FacadeManager를 우회하여 매니저를 직접 초기화하지 않는가
- **Singleton**: `Singleton<T>` 기반으로 매니저에 접근하는가
- **Observer**: 이벤트 통신이 EventManager를 통하는가
- **State**: 상태별 로직이 IGameState 구현체 내에 캡슐화되는가
- **Strategy**: 입력/카메라/BGM 핸들러가 인터페이스 기반으로 교체 가능한가
- **Object Pooling**: 반복 생성 오브젝트가 풀링을 사용하는가
- **Provider (DI)**: 데이터 소스가 인터페이스 기반 Provider를 사용하는가
- **Partial Class**: 대형 매니저가 도메인별로 파일 분리되어 있는가

### 10. 초기화/해제 시퀀스 (Section 3)
- `Init()`에서 리소스 획득, `Release()`에서 해제가 쌍을 이루는가
- 이벤트 리스너 등록/해제가 올바르게 짝지어지는가
- 캐시 정리가 `Release()`에서 이루어지는가

## 출력 형식

### 위반 사항 발견 시:
```
## 아키텍처 검토 결과

### 검토 기준
- 브랜치: `feature/xxx` (dev 대비)
- 변경 파일 수: X개 (.cs 파일 기준)

---

### 파일명.cs
- **[위반]** 라인 XX: `직접 FindObjectOfType 사용` -> Singleton<T>.Instance를 통해 접근해야 합니다 (Section 1, 10)
- **[위반]** 라인 XX: `Tier 0 매니저가 Tier 1에 의존` -> 의존성 Tier 구조 위반 (Section 2)
- **[경고]** 라인 XX: `이벤트 리스너 등록 후 해제 누락` -> Release()에서 RemoveListener 필요 (Section 3, 4)
- **[권장]** 라인 XX: `직접 Instantiate 사용` -> ObjectManager.Spawn 사용 권장 (Section 6-1)

### 파일명2.cs
- 아키텍처 위반 없음

---

### 총평
- 총 X개 파일 검토
- 위반: X건 / 경고: X건 / 권장: X건
- 주요 이슈 요약 (있는 경우)
```

### 위반 없을 시:
```
## 아키텍처 검토 결과

### 검토 기준
- 브랜치: `feature/xxx` (dev 대비)
- 변경 파일 수: X개 (.cs 파일 기준)

모든 변경 파일이 ARCHITECTURE.md 아키텍처를 올바르게 따르고 있습니다.
```

## 심각도 분류

| 심각도 | 설명 | 예시 |
|--------|------|------|
| **[위반]** | 아키텍처 원칙을 명확히 위반. 반드시 수정 필요 | 순환 의존성, Tier 구조 위반, 매니저 우회 접근 |
| **[경고]** | 잠재적 문제. 의도적이지 않다면 수정 권장 | 리스너 해제 누락, 불완전한 생명주기 |
| **[권장]** | 더 나은 아키텍처 준수를 위한 개선 제안 | 풀링 미사용, Provider 패턴 미적용 |
