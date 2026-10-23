# 홀 교차 수정·2층 마감 01 검증 · 2026-10-23

작업 브랜치: `art/professor-house-upper-finish`. Unity 2022.3.62f3. [실제 화면 11장](ProfessorHouseUpperFinish/index.html) · [제작·재생성 방법](../Art/ProfessorHouseUpperFinish.md).

| 검사 | 결과 | 근거 |
| --- | --- | --- |
| 수정 전 재현 | 계단 아래 풍경화와 계단·몰딩, 남성 초상화와 난간의 교차 | [초기 BVH 검사](ProfessorHouseUpperFinish/HallIntersectionsBefore.json) |
| 수정 후 홀 | 장식 부품 472개 × 승인 건축 261개 메시, 표면 교차 0 | [홀 교차 검사](ProfessorHouseUpperFinish/HallPlacement.json) |
| 새 2층 가구 | 부품 2,725개 × 승인 건축 261개 메시, 표면 교차 0 | [2층 교차 검사](ProfessorHouseUpperFinish/UpperPlacement.json) |
| 커튼·창 장식 | 방별 가구/창 장식 네 그룹 쌍, 교차 0 | [장식 간격](ProfessorHouseUpperFinish/DecorationClearance.json) |
| 구조 임포트 | 116개 메시의 Unity 축·스케일 경계 일치 | [구조](ProfessorHouseUpperFinish/ImportBounds.txt) |
| 마감 임포트 | 홀 7개/69,574삼각형/충돌 9개, 2층 14개/253,668삼각형/충돌 39개 | [홀](ProfessorHouseUpperFinish/HallFinishImport.txt) · [2층](ProfessorHouseUpperFinish/UpperFinishImport.txt) |
| 전체 이동 | 182구간·캡슐 1,599표본, 문 53개 개방/교수 경계 1개 폐쇄 | [NavMesh](ProfessorHouseUpperFinish/Navigation.txt) |
| 새 방 내부 접근 | 책상 뒤·독서 자리·각 침대 앞 8경로, 캡슐 144표본, 실패 0 | [실내 이동](ProfessorHouseUpperFinish/RoomAccess.txt) |
| 실제 Lucy 이동 | 첫 안내·숨바꼭질·유물 사고 피신·오른쪽 계단, 186웨이포인트 | [플레이 기록](ProfessorHouseUpperFinish/PlayerTraversal.txt) |
| EditMode | 18/18 통과: 프로젝트 17 + Addressables 예제 1 | [XML](ProfessorHouseUpperFinish/EditMode.xml) |
| PlayMode | 8/8 통과: 연결 씬 이동·재진입 2 + 기존 회귀 6 | [XML](ProfessorHouseUpperFinish/PlayMode.xml) |
| 구조·Chalk 계약 | 실패 0 | [정적 검사](ProfessorHouseUpperFinish/StaticChecks.txt) |
| 검증 입력 | 실제 소스·자산·설정·문서·결과 해시 | [스냅샷](ProfessorHouseUpperFinish/SourceSnapshot.json) |

교차 검사는 선택한 장식과 건축 표면을 대상으로 한다. 바닥 지지·벽 마감·창틀의 붙임 접촉은 별도 영역이며, 모든 메시 내부 포함 관계와 모든 가구 쌍을 완전 탐색한 증거로 해석하지 않는다. Unity 화면을 여러 방향에서 확인해 침대 머리판의 이음선·가구 모서리·액자/커튼 간격과 모서리 보조창 마감을 보완했다.

확정 도면 SHA-256은 `c10cae9559129d7618d6f6971bfab6e8b615bec9ecafee22824a8c7b01bf7fd9`로 유지한다. 원본 연결 Blender와 문·층고·계단 치수는 바꾸지 않았다. 검사와 캡처는 제작용 연결 씬 기준이다. 제품 씬 통합·라이트맵 베이크·출시 성능 검증·플레이어 빌드는 이번 범위에 포함하지 않는다.
