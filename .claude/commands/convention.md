# Unity 코딩 컨벤션 검토

`UNITY_CODING_CONVENTION.md` 기준으로 C# 코드를 검토합니다.

## 사용법

- `/convention` - Git 변경 파일 검토 (staged + unstaged + untracked)
- `/convention [파일경로]` - 특정 파일 검토 (예: `Assets/Scripts/Player.cs`)
- `/convention [폴더경로]` - 특정 폴더의 모든 .cs 파일 검토 (예: `Assets/Scripts/`)

## 작업 절차

### 검토 대상: $ARGUMENTS

1. **검토 대상 파일 확인**
   - **인자가 없는 경우**: `git status --short`로 변경된 파일 목록 확인 (staged, unstaged, untracked 모두 포함)
   - **인자가 있는 경우**:
     - 파일이면 해당 파일 검토
     - 폴더면 해당 폴더 내 모든 `.cs` 파일 검토
   - `.cs` 파일만 필터링하여 검토 대상 선정
   - 검토할 파일이 없으면 알림 후 종료

2. **컨벤션 문서 참조**
   - `UNITY_CODING_CONVENTION.md` 파일 읽기

3. **각 파일 검토** (아래 체크리스트 기준)
   - **인자가 없는 경우**: 변경된 `.cs` 파일 전체 코드 검토
   - **인자가 있는 경우**: 파일 전체 코드 검토

4. **결과 리포트 출력**

## 검토 체크리스트

### 네이밍 규칙
- 클래스/구조체/Enum: PascalCase
- public/protected 멤버: PascalCase
- private 필드: camelCase (`_`, `m_` 접두어 사용 금지)
- 상수: PascalCase + `static readonly` 권장 (const보다)
- 이벤트: `On` + 동사 (예: `OnPlayerDeath`)
- 비동기 메서드: `Async` 접미어
- UI 클래스: `UI_` 접두어

### 금지 항목
- 람다식 사용 금지 (DOTween, UniTask 등 외부 라이브러리는 허용)
- LINQ 사용 금지 (.Where, .Select, .FirstOrDefault, .Sum 등)
- `Update()`, `FixedUpdate()`, `LateUpdate()` 사용 금지
- 코루틴 사용 금지 (StartCoroutine, IEnumerator 반환)
- `Camera.main` 직접 사용 금지 (CameraManager 사용)
- 매직 넘버/스트링 금지 (상수로 정의)

### var 사용
- 타입이 명확한 경우만 허용 (new, 리터럴)
- `var data = GetData();` 형태 금지 (반환 타입 불명확)

### 구조
- 한 파일 = 한 클래스
- 파일명과 클래스명 일치
- 멤버 정렬 순서: 상수 > static > public > SerializeField > private > 프로퍼티 > Unity콜백 > Initialize > public메서드 > private메서드 > Release

### Unity 특화
- `Awake`/`Start`는 프레임워크/씬 초기화에만 허용
- 일반 객체는 `Initialize()` 메서드로 초기화
- `SerializeField private` 우선 사용
- `GetComponent` 캐싱 필수

## 출력 형식

### 위반 사항 발견 시:
```
## 컨벤션 검토 결과

### 파일명.cs
- **[위반]** 라인 XX: `private int _count;` -> private 필드에 `_` 접두어 사용 금지
- **[위반]** 라인 XX: LINQ 사용 감지 (`.Where()`)
- **[권장]** 라인 XX: `var data = GetData();` -> 명시적 타입 선언 권장

### 파일명2.cs
- 위반 사항 없음

### 총평
- 총 X개 파일 검토
- 위반: X건 / 권장: X건
```

### 위반 없을 시:
```
## 컨벤션 검토 결과
모든 변경 파일이 컨벤션을 준수합니다
```
