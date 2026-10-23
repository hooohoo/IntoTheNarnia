# IntoTheNarnia

Unity 기반 나니아 탐험 프로젝트. 교수의 집에서 Lucy 이동·대화·문 상호작용·퀘스트를 실행한다.

## 실행 환경

| 항목 | 기준 |
| --- | --- |
| Unity | 2022.3.62f3 |
| 렌더링 | URP 14.0.12, Linear |
| 리소스 | Addressables 1.22.3, 로컬 콘텐츠 빌드 |
| Cinemachine / TMP | 2.10.5 / 3.0.7 |
| JSON / Timeline | Newtonsoft 3.2.2 / Timeline 1.7.7 |
| 비동기 | UniTask 2.5.10, 취소 가능한 UniTask |
| 입력 | 기존 Unity Input Manager + 상태별 InputHandlerBase |

Git LFS를 설치한 뒤 `git lfs install`과 `git lfs pull`로 바이너리 에셋을 받는다.

`Assets/@Project/00.Scenes/Title.unity`에서 시작한다. 교수의 집 직접 실행도 지원한다. Addressables Play Mode는 `Use Asset Database` 또는 콘텐츠 빌드 후 `Use Existing Build`를 사용한다. 후자는 현재 활성 빌드 타깃과 같은 타깃의 콘텐츠가 필요하다.

## 구조와 작업 기준

[CLAUDE.md](CLAUDE.md) → [ARCHITECTURE.md](ARCHITECTURE.md) → [컨벤션](UNITY_CODING_CONVENTION.md) 순서로 읽는다. [Chalk 통합 점검](Docs/Architecture/ChalkParity.md)에 비교 범위·반영 항목·프로젝트별 차이의 근거를 기록한다.

자체 에셋은 Chalk와 같은 `00.Scenes`, `01.Scripts`, `02.Prefabs`, `03.Sprites`, `04.Materials`, `05.Animations`, `06.Data`, `07.Sounds`, `08.Fonts`, `09.Shaders`, `10.Models`에 둔다. 프로젝트 설정 에셋은 `Settings`, 에디터 도구는 `Editor`, 검증은 `Tests`, 예제는 별도 어셈블리 `Samples`에 둔다. 런타임 자체 콘텐츠에는 Resources 로드를 사용하지 않는다.

## 검증과 빌드

- `python3 Tools/validate_structure.py`: 원본 스크립트 GUID, 에셋 참조 및 교수의 집 메시 계약 검사.
- `python3 Tools/validate_chalk_contracts.py`: 엔진·공통 패키지·설정·리소스·컨벤션 정적 검사.
- Unity Test Runner: EditMode와 PlayMode. PlayMode는 실제 Addressables 소유권, 씬 프리로드, UI 버튼, 퀘스트·재진입을 검사한다.
- `Narnia/Validate Project Structure`, `Narnia/Validate Addressable Contracts`: 씬/프리팹·라벨 계약 검사.
- `Narnia/Build/Addressable Content`: 활성 타깃의 로컬 콘텐츠 빌드.
- `Narnia/Build/macOS Player`: 등록된 게임 씬과 Addressables 콘텐츠를 함께 빌드. CLI 진입점은 `NarniaBuild.BuildMac`이며 기본 출력은 `Builds/macOS/IntoTheNarnia.app`이다.

교수의 집 현재 기준점의 재검증은 [2026-10-23 검증](Docs/Validation/ProfessorHouseBaseline.md), 초기 재구성 결과는 [v2 검증](Docs/Validation/ProfessorHouseV2.md)에 기록한다. [Chalk 통합 검증](Docs/Validation/ChalkParity.md)과 [2021 기준 검증](Docs/Validation/2026-10-21.md)은 각 전환 당시의 이력이다.

## 콘텐츠 현황

플레이 흐름은 Title → LoadingScene → Professor_House다. TheSiblingsRoom/TheWardrobeRoom은 기존 콘텐츠 자리표시 씬이다. Title의 Continue/Settings는 원본의 미구현 진입점을 보존한다. Main/Test/WinterForest는 빌드에서 제외한다. 이 콘텐츠 상태를 공통 프레임워크 통합 완료와 혼동하지 않는다.

## 아트 레퍼런스

Lucy와 교수의 집은 [아트 레퍼런스](Docs/Art/README.md)의 v2 시안을 따른다. 교수의 집 메인 홀·계단·2층 회랑은 다크 월넛, 둥근 난간과 밝은 석재 바닥으로 교체했다. [제작 원본과 재구성 도구](Docs/Art/ProfessorHouseV2.md)를 함께 관리한다. Lucy 모델과 별도 옷장방의 교체는 후속 범위다.

2026-10-22에 [저택 층별 도면 03](Docs/Art/ProfessorHouse_ApprovedLayout_v1/index.html)을 확정했다. [확정 기준](Docs/Art/ProfessorHouse_ApprovedLayout_v1/README.md)의 방 배치·계단·정원·장면별 동선을 이후 모델링에 적용한다. 현재 Unity v2 홀과 전체 확정 배치의 구현 상태는 구분한다.

## Git

항상 새 작업 브랜치에서 수정한다. main은 병합 전용이며 작업 커밋 1개 → `git merge --no-ff` 1개 → `git push --atomic origin main <작업브랜치>` 1회로 통합한다. PR은 선택이다. 이력 재작성·force push를 하지 않는다.
