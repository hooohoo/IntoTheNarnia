# 중앙 홀 마감 01 검증 · 2026-10-23

작업 브랜치: `art/professor-house-hall-finish`. Unity 2022.3.62f3. [실제 화면](ProfessorHouseHallFinish/index.html) · [구현 내용](../Art/ProfessorHouseHallFinish.md).

| 검사 | 결과 | 증거 |
| --- | --- | --- |
| 구조·Chalk 공통 계약 | 실패 0 | [정적 검사](ProfessorHouseHallFinish/StaticChecks.txt) |
| 기본 구조 임포트 | 기존 메시 116개 축·스케일 검사 통과 | [경계 검사](ProfessorHouseHallFinish/ImportBounds.txt) |
| 새 가구·장식 임포트 | 7개 메시 경계 일치, 69,574삼각형, BoxCollider 9개 | [마감 임포트](ProfessorHouseHallFinish/HallFinishImport.txt) |
| 가구 추가 후 이동 | 182개 경로 구간·캡슐 표본 1,599곳 실패 0 | [NavMesh](ProfessorHouseHallFinish/Navigation.txt) |
| 실제 루시 이동 | 첫 안내·숨바꼭질·유물 사고·오른쪽 계단, 186개 웨이포인트 | [플레이어 기록](ProfessorHouseHallFinish/PlayerTraversal.txt) |
| EditMode | 18/18 통과: 프로젝트 17 + Addressables 예제 1 | [XML](ProfessorHouseHallFinish/EditMode.xml) |
| PlayMode | 8/8 통과: 이동·재진입 2 + 기존 회귀 6 | [XML](ProfessorHouseHallFinish/PlayMode.xml) |
| 실행 대상 기록 | 소스·자산·설정 해시 | [스냅샷](ProfessorHouseHallFinish/SourceSnapshot.json) |

PlayMode는 기존 InputManager → PlayerController → NavMeshAgent 입력 경로를 실행하고 카메라 방향·충돌, 씬 재진입 및 Addressables 소유권 정리를 검사했다. 새 가구의 박스 충돌체를 포함해 NavMesh를 다시 구웠다. 열린 출입구 53곳과 교수 구역의 닫힌 경계 1곳, 후정/중정 상층 공백과 S2→K0 지름길 차단도 유지한다.

실제 그래픽 모드 Unity에서 홀 4장과 다른 구역 3장을 촬영했다. 그림 텍스처는 생성된 원본 바이트 그대로이며 UV로 4개 그림을 사용한다. 기존 승인 도면·연결 모형·시안과 원래 제품 씬, Build Settings, NavMesh Area, 패키지 구성은 변경하지 않았다. URP는 마감용 렌더러 참조 1개를 추가했으며 기본 렌더러와 LDR 설정을 유지한다.

이번 마감은 실시간 조명이다. 라이트맵 베이크·모바일 성능·제품 플레이어 빌드를 검사한 결과로 해석하지 않는다. 이전 [연결 구조 검증 기록](ProfessorHouseConnected.md)은 그 시점의 증거로 보존한다.
