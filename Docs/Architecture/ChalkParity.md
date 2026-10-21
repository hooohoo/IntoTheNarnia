# Chalk 기준 통합 점검 · 2026-10-21

Chalk의 CLAUDE.md, Roles/Convention, Roles/Architecture, 실제 매니저·Packages·ProjectSettings를 대조하여 공통 기준을 Narnia에 적용했다. 이전의 Resources 유지·Addressables 추후 도입 결정은 폐기한다. 구현과 검증은 새 작업 브랜치 `refac/chalk-parity-20261021`에서 수행한다.

| 영역 | 이전 Narnia | 적용 결과 |
| --- | --- | --- |
| 엔진·렌더링 | Unity 2021.3.6f1, Built-in/Gamma | Unity 2022.3.62f3, URP 14.0.12/Linear, 선형 조명·색온도, 머티리얼 344개 변환 |
| 패키지 | Addressables 없음, 구버전 공통 패키지 | Addressables 1.22.3, Cinemachine 2.10.5, TMP 3.0.7, Newtonsoft 3.2.2, UniTask 2.5.10; 공통 lock 의존성 60개 버전 일치 |
| 리소스 | Resources.Load(All), Dictionary.Clear | AddressableManager 비동기 프리로드·캐시, 소유자별 공유·반환, 진행 중 이탈·실패·재진입 처리 |
| 그룹·빌드 | 그룹/콘텐츠 빌드 없음 | Title/Loading/ProfessorHouse/Story 라벨 4개, 엔트리 11개, 공통 의존성 Dependencies 분리, 로컬 콘텐츠 동반 플레이어 빌드 |
| 씬·부트 | 실행 순서 의존, 로드 직후 입력 허용 | 상태 등록·Loading → 프리로드 → Screen 구성 → Ready; 실패 시 입력 차단, 이탈 시 취소·정리 |
| UI | 씬 배치 HUD·문자열 탐색 | UI_Base/Scene/Popup/Sub, Addressable 프리팹 5개, Scene 캐시·Popup 정렬·Sub 풀·소유권, 1920×1080 화면 스케일 |
| 객체 | ObjectManager가 씬 참조만 보관 | OBJ_Base Spawn/Despawn 풀, 씬 소유 인스턴스 정리 후 리소스 소유권 반환 |
| 상태·입력 | 전역 일괄 등록·InputPolicy | 씬별 등록, InputHandlerBase, 상태별 핸들러, 중첩 팝업·재진입·포인터 소유권 |
| 데이터 | Resources TextAsset | IDialogueProvider.LoadAsync, Addressable JSON, 제공자 교체·캐시 정리 |
| 이벤트·DI·로그 | 개별 구현 | enum 이벤트/IListener/PostNotification, InjectUtil, 바인딩 검증, LogUtil·LogTag |
| 디렉터리 | Resources 중첩, 다른 번호·Framework | 00.Scenes~10.Models, Manager/Facade·Interface·Manager, Base·Singleton·Util, Editor/Tests/Samples 분리 |
| 에디터·물리·오디오 | 공통 기본값 차이 | Force Text·Visible Meta·LF·Sprite Atlas V2, editorconfig, Audio·3D/2D Physics·Common Burst·SBP 설정 통일 |
| 플랫폼 공통 설정 | Unity 2021 PlayerSettings 잔존 | Unity 2022 설정 형식, DEBUG_LOG, WebGL 1920×1080/Minimal/Brotli·fallback·메모리·배칭·스트리핑 통일 |
| Git·자동 검사 | 적용되지 않는 LFS 폴더 패턴 | 확장자 기반 LFS·기존 자산 재정규화, 생성물 제외, 구조/공통 계약 CI |

비교 근거는 [ChalkSettingsComparison.json](ChalkSettingsComparison.json)에 ProjectSettings 36개 항목, 직접 패키지 합집합 57개, 공통 lock 패키지 60개 및 PlayerSettings의 남은 차이 18개를 기록한다. 파일 해시와 필드별 예외 사유를 포함한다.

## 프로젝트 고유값과 적용 경계

- 제품명·앱 ID·Cloud 조직·서명·릴리스 번호는 Narnia 값을 유지한다. 기존 생성 템플릿 정보는 프로젝트 이력이다.
- 씬·퀘스트·태그/레이어·NavMesh Area/agent·품질 단계·40m 그림자 예산은 기존 게임 콘텐츠다. Chalk의 튜터 콘텐츠 값은 이식하지 않는다.
- 독립 플레이어의 16:9 해상도와 불투명 프레임버퍼를 유지한다. Chalk의 Flutter 투명 합성, NGO/UTP·Linux 서버, AWS/CDN·외부 HTTP 연결은 Narnia에서 사용하지 않는다. Addressables 프로필은 로컬이다.
- UniTask 버전은 같고 설치 경로만 Assets 소스/UPM으로 다르다. 현재 UI의 Animator를 DOTween으로 교체할 기능 요구가 없어 DOTween 플러그인과 define은 추가하지 않는다.
- Narnia 콘텐츠가 사용하는 Navigation/Rigging/Polybrush는 유지한다. 미사용 Collab/Visual Scripting은 패키지·생성 데이터·설정과 함께 제거했다. MCP는 공통 coplay 버전을 사용하며 Chalk의 두 번째 MCP 연결은 중복 설치하지 않는다.
- Samples는 autoReferenced=false인 별도 어셈블리다. Editor-only 제한은 Test/WinterForest의 기존 UnityEvent 대상 역직렬화를 깨뜨리므로 사용하지 않는다. 활성 빌드 씬에 Samples 컴포넌트가 없는지 ContentValidation으로 검사하며 샘플 로더는 UNITY_EDITOR 안에 둔다.
- Asset Store/TMP 등 외부 소유 폴더와 에셋 이름은 유지한다. 자체 Addressable 주소는 확장자 없는 파일명이며 동명 충돌을 검사한다.

## 검증과 통합

[실행 보고서](../Validation/ChalkParity.md)에 자체 EditMode 17개, 실제 번들 PlayMode 5개, macOS 플레이어 빌드·실행·화면 캡처, 스크립트 GUID/메시/씬 참조 결과를 남겼다. 이전 Unity 2021 검증과 구분한다.

새 작업 브랜치의 작업 커밋 1개 → main의 `--no-ff` merge 1개 → main과 작업 브랜치를 함께 올리는 atomic push 1회로 통합한다. main에서 일반 작업 커밋을 만들거나 공개 이력을 재작성하지 않는다. PR은 선택이다.
