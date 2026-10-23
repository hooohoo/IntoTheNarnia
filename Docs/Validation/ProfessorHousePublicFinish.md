# 1층 공용 공간 검증 · 2026-10-23

작업 브랜치: `art/professor-house-public-rooms`. Unity 2022.3.62f3. [Unity 화면 11장·가구 배치도](ProfessorHousePublicFinish/index.html) · [제작 기록](../Art/ProfessorHousePublicFinish.md).

| 검사 | 결과 | 근거 |
| --- | --- | --- |
| 가구와 원래 건축 | 2,612개 부품 × 261개 건축 메시, 표면 교차 0 | [BVH 검사](ProfessorHousePublicFinish/PlacementValidation.json) |
| 가구·커튼·그림 간격 | 선택한 14개 그룹 쌍, 교차 0 | [장식 간격](ProfessorHousePublicFinish/DecorationClearance.json) |
| 임포트 축·스케일 | 새 25개 메시 경계 일치; 기존 구조·마감도 일치 | [새 레이어](ProfessorHousePublicFinish/PublicFinishImport.txt) · [구조](ProfessorHousePublicFinish/ImportBounds.txt) |
| 새 가구 접근·서사 동선 | 접근 11경로 + 직접 접근 4구간, 515캡슐, 실패 0 | [접근 검사](ProfessorHousePublicFinish/PublicAccess.txt) |
| 전체 연결 | 182구간·1,599캡슐, 열린 문 53·닫힌 교수 경계 1, 실패 0 | [이동 검사](ProfessorHousePublicFinish/Navigation.txt) |
| 이전 구역 접근 | 2층 8경로·144캡슐, 전시실 4경로·133캡슐, 실패 0 | [2층](ProfessorHousePublicFinish/RoomAccess.txt) · [전시실·사고 축](ProfessorHousePublicFinish/GalleryAccess.txt) |
| 실제 Lucy 이동 | 4경로·186웨이포인트, 이동 중 순간이동 없음 | [입력 이동](ProfessorHousePublicFinish/PlayerTraversal.txt) |
| EditMode | 18/18 통과 | [XML](ProfessorHousePublicFinish/EditMode.xml) |
| PlayMode | 8/8 통과 | [XML](ProfessorHousePublicFinish/PlayMode.xml) |
| 구조·Chalk 계약 | 실패 0 | [정적 검사](ProfessorHousePublicFinish/StaticChecks.txt) |
| 검증 입력·화면 | 소스·모델·설정·문서·결과 해시 | [스냅샷](ProfessorHousePublicFinish/SourceSnapshot.json) |

첫 접근 검사에서 도서실 창가 원탁 모서리를 스치는 표본 하나를 발견해 원탁을 이동했다. 최종 검사는 캐릭터 반경 0.22 m와 기존 NavMesh 설정을 유지한 채 통과했다. 화면에서 의자 윗부분의 이음선과 천장등 고리의 분절을 확인해 단일 곡면으로 수정했고, 벽난로 상부를 천장에 연결했다. 최종 [카메라 위치](ProfessorHousePublicFinish/Cameras.json)의 시야각은 70도, 이미지 크기는 1536×1024다.

BVH 검사는 선택한 가구와 건축 표면 및 명시한 장식 그룹 쌍의 교차 검사다. 의도된 벽 마감·바닥 접촉은 제외하며, 전체 가구 쌍이나 완전히 내부에 포함된 형상의 무교차까지 증명하지 않는다. 창가 접근 검사는 NavMesh 경로와 실제 충돌 캡슐을 함께 사용한다. 사고 축 검사는 기존 갑옷으로 향하는 배치 확인이며 파손 연출 테스트가 아니다.

확정 도면 SHA-256은 `c10cae9559129d7618d6f6971bfab6e8b615bec9ecafee22824a8c7b01bf7fd9`로 유지한다. Unity가 자동 수정한 VSCode 파일과 QualitySettings의 AA 값은 기준 상태로 복구했고, 생성 YAML·테스트 XML의 줄 끝 공백만 정리했다. 검증 당시 해시는 스냅샷에 별도 보존한다. 최종 씬 생성·화면 촬영·PlayMode·EditMode 실행은 모두 종료 코드 0으로 정상 종료했다. 플레이어 빌드·조명 베이크·출시 성능 검증은 이번 범위에 포함하지 않았다.
