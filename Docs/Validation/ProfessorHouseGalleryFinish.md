# 전시실·연결 복도와 서재 높이 검증 · 2026-10-23

작업 브랜치: `art/professor-house-gallery-finish`. Unity 2022.3.62f3. [실제 화면 11장·서재 전후 비교](ProfessorHouseGalleryFinish/index.html) · [제작 기록](../Art/ProfessorHouseGalleryFinish.md).

| 검사 | 결과 | 근거 |
| --- | --- | --- |
| 서재 실측 | 천장 4.423 m / 보 아래 최소 4.158 m, 확정 구조 유지 | [BVH 측정](ProfessorHouseGalleryFinish/StudyHeight.json) |
| 전시실·복도 장식 | 824개 부품 × 승인 건축 261개 메시, 표면 교차 0 | [배치](ProfessorHouseGalleryFinish/GalleryPlacement.json) |
| 서재 포함 2층 가구 | 3,215개 부품 × 건축 261개 메시, 표면 교차 0 | [배치](ProfessorHouseGalleryFinish/UpperPlacement.json) |
| 장식끼리의 간격 | 전시실 4쌍, 2층 6쌍, 교차 0 | [전시실](ProfessorHouseGalleryFinish/GalleryDecorationClearance.json) · [2층](ProfessorHouseGalleryFinish/UpperDecorationClearance.json) |
| 사고 축 | 창 안쪽 광선이 다른 가구보다 먼저 투구 메시·갑옷 충돌체에 닿음 | [메시](ProfessorHouseGalleryFinish/AccidentAxis.json) · [Unity](ProfessorHouseGalleryFinish/GalleryAccess.txt) |
| 임포트 축·스케일 | 구조 116개, 홀 7개, 2층 16개, 전시실·복도 22개 메시 경계 일치 | [구조](ProfessorHouseGalleryFinish/ImportBounds.txt) · [전시실](ProfessorHouseGalleryFinish/GalleryFinishImport.txt) · [2층](ProfessorHouseGalleryFinish/UpperFinishImport.txt) |
| 전체 이동 | 182구간, 캡슐 1,599표본, 개방 문 53/닫힌 교수 경계 1, 실패 0 | [NavMesh](ProfessorHouseGalleryFinish/Navigation.txt) |
| 가구 접근 | 전시실 4경로·133캡슐, 2층 8경로·144캡슐, 실패 0 | [전시실](ProfessorHouseGalleryFinish/GalleryAccess.txt) · [2층](ProfessorHouseGalleryFinish/RoomAccess.txt) |
| 실제 Lucy 이동 | 안내·숨바꼭질·사고 피신·동쪽 계단 4경로, 186웨이포인트 | [플레이 기록](ProfessorHouseGalleryFinish/PlayerTraversal.txt) |
| EditMode | 18/18 통과, 프로젝트 17 + Addressables 예제 1 | [XML](ProfessorHouseGalleryFinish/EditMode.xml) |
| PlayMode | 8/8 통과, 연결 이동·재진입 2 + 기존 회귀 6 | [XML](ProfessorHouseGalleryFinish/PlayMode.xml) |
| 구조·Chalk 계약 | 실패 0 | [정적 검사](ProfessorHouseGalleryFinish/StaticChecks.txt) |
| 검증 입력 | 소스·모델·설정·문서·화면·검증 결과 해시 | [스냅샷](ProfessorHouseGalleryFinish/SourceSnapshot.json) |

층고는 생성된 마감 표면의 수직 광선 교차로 측정했다. Unity에서는 내보낸 메시의 경계값 일치도 별도로 검사한다. 시안 한 장의 절대 높이를 실측한 결과는 아니다. 서재 전후 비교는 카메라 위치·목표·70도 시야각이 같다.

BVH 배치 검사는 선택한 가구와 건축 표면, 명시한 장식 그룹 쌍의 교차 검사다. 바닥 접촉·벽 마감·붙임 창틀은 의도된 결합이며 전체 메시의 내부 포함이나 모든 가구 쌍의 무교차를 증명하지 않는다. 화면 검토로 벤치 방향, 석재 아치 앞 보의 끝, 서재 몰딩 중첩, 반쪽 창 마감, 천장 과노출도 보완했다. 사고 축 검사는 배치 검증이며 유리 파손·갑옷 낙하의 실행 테스트는 아니다.

확정 도면 SHA-256은 `c10cae9559129d7618d6f6971bfab6e8b615bec9ecafee22824a8c7b01bf7fd9`로 유지한다. 연결 구조 원본과 제품 씬/Build Settings는 변경하지 않았다. Unity가 자동 수정한 VSCode 파일과 QualitySettings의 AA 값은 기준 상태로 복구했다. 검증 뒤 Unity YAML의 줄 끝 공백만 정리했으며 원본 검증 해시는 스냅샷에 함께 기록한다. 테스트 XML의 내장 스택 추적 끝 공백도 같은 방식으로 보존·정리한다. PlayMode XML은 8개 통과로 저장됐으나 검증용 Unity 프로세스는 종료 로그를 남긴 뒤 macOS 종료 대기 상태(UE)에 머물렀다. 프로젝트 잠금과 작업 파일 해제 확인 후 EditMode를 별도로 실행했다. 이 실행의 프로세스 종료 코드 0을 주장하지 않는다. 플레이어 빌드·베이크·출시 성능 검증은 후속 범위다.
