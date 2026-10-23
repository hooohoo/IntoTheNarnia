# 교수의 집 · 전체 연결 구조 Unity 검증 — 2026-10-23

작업 브랜치: `feat/professor-house-connected-walkthrough`. 승인된 시안 7장과 구조 원본을 프로젝트에 등록하고, `Professor_House_Connected.unity`에서 50개 공간·54개 출입구의 이동 관계를 확인했다. [구현·재생성 방법](../Art/ProfessorHouseConnected.md) · [실제 Unity 화면](ProfessorHouseConnected/index.html).

| 검사 | 이번 결과 | 기록 |
| --- | --- | --- |
| 구조·Chalk 계약 | 실패 0, 원본 스크립트 46개 GUID 보존 | [정적 검사](ProfessorHouseConnected/StaticChecks.txt) |
| Unity 임포트 | 메시 116개 축·미터 스케일·월드 경계 일치 | [임포트](ProfessorHouseConnected/ImportBounds.txt) |
| 실제 NavMesh·충돌 | 순서 경로 182구간, 캡슐 표본 1,599곳 실패 0 | [이동 검사](ProfessorHouseConnected/Navigation.txt) |
| 출입구와 경계 | 열린 문 53곳, 닫힌 교수 구역 1곳 통과 | 같은 이동 검사 |
| 공간 연결 제약 | 후정·중정 상층은 비어 있음, S2→K0 직접 지름길 없음 | 같은 이동 검사 |
| 실제 플레이어 이동 | 4개 경로, 186개 순서 웨이포인트 통과 | [입력 이동 기록](ProfessorHouseConnected/PlayerTraversal.txt) |
| EditMode | 18개 통과: 프로젝트 17 + Addressables 예제 1 | [XML](ProfessorHouseConnected/EditMode.xml) |
| PlayMode | 8개 통과: 새 검증 2 + 기존 회귀 6 | [XML](ProfessorHouseConnected/PlayMode.xml) |
| 참고 이미지 보존 | 승인 PNG 7개 바이트 해시 일치 | [승인 기록](../Art/ProfessorHouse_FinishedConcepts_v1/ProjectApproval.json) |
| 검사 대상 | 파생 자산·코드·원본·설정 해시 | [스냅샷](ProfessorHouseConnected/SourceSnapshot.json) |

PlayMode는 기존 InputManager → PlayerController → NavMeshAgent 경로로 이동한다. 첫 안내는 2층 아이들 방, 숨바꼭질과 유물 사고 경로는 다락 옷장방, 별도 오른쪽 계단 경로는 동측 회랑에 도달했다. 경로 시작 위치 설정 외에 이동 중 Warp/순간이동을 사용하지 않는다. 경로 끝에서 카메라의 정상 방향과 고정 충돌체 내부 침범 여부도 확인했다. 씬 재진입은 카메라 입력 델리게이트 복원, 플레이어 정리, Addressables 소유권 반환을 검사한다.

캐릭터 1.20 m·반경 0.22 m는 모형과 같은 공간 검증 가정이다. 기존 Lucy 프리팹의 생성된 인스턴스만 크기를 맞추며 원본 프리팹과 최종 체형은 바꾸지 않는다. 이번 저작용 씬에서는 이동을 NavMesh가 소유하고, 기존 아바타의 중복 캡슐은 카메라 간섭을 막기 위해 인스턴스에서 비활성화한다. 캡슐 공간 검사는 별도의 1.20 m/0.22 m 물리 샘플이다.

이번 검증 씬은 제품 Build Settings에 추가하지 않았다. 기존 `Professor_House`의 GUID, 문 퀘스트, Title 진입 흐름, NavMesh Area와 Addressables 그룹은 보존했다. 고정 환경·NavMesh는 씬 직렬화 참조가 소유하며 Lucy·UI는 기존 ProfessorHouse 라벨을 사용한다. 일반 방 천장·지붕·가구·창밖 풍경·최종 조명과 동적 문/씬 전환은 후속 제작이다. 이번에는 제품 플레이어 빌드를 다시 만들지 않았다.
