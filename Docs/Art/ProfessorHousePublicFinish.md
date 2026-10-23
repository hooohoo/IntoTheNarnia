# 교수의 집 · 1층 공용 공간 마감 01

2026-10-23. `Professor_House_Connected`의 C1 응접실·A 현관·C2 식당·D2 공용 도서실에 가구와 기본 마감을 추가했다. [실제 Unity 화면 11장·배치도](../Validation/ProfessorHousePublicFinish/index.html) · [검증](../Validation/ProfessorHousePublicFinish.md).

## 형태와 배치

승인된 [C+D 보드](ProfessorHouse_DesignReview_v1/Boards/04_CD_Living_Study_v1.png)의 벽난로·월넛·커튼·식탁 분위기를 둥근 로우폴리 형태로 적용한다. 보드의 이전 방 번호·연결 구조보다 [확정 도면](ProfessorHouse_ApprovedLayout_v1/README.md)을 우선한다. 새 방 배치를 제안하거나 영화의 정확한 내부를 복원한 작업이 아니다.

| 구역 | 적용한 구성 | 유지한 연결 |
| --- | --- | --- |
| C1 응접실 | 둥근 소파·안락의자 2개·낮은 타원탁·석재 벽난로·콘솔·그림·커튼 | 서쪽 정원문, 북쪽 전시실 문, 동쪽 현관 문 |
| A 현관 | 석재 타일·긴 러너·양옆 벤치·콘솔·초상화·천장등 | 정문에서 중앙 홀로 가는 축, 응접실과 식당의 양쪽 접근 |
| C2 식당 | 타원형 10인 식탁·의자 10개·식기·린넨·꽃·식기장·커튼 | 현관, 북쪽 동쪽 연결 복도, 동쪽 전면 갤러리의 세 문 |
| D2 공용 도서실 | 서가 6개·공동 독서 테이블과 의자 4개·창가 안락의자 2개·원탁·창 아래 벤치 | 남쪽 전시실, 동쪽 수집품 갤러리 |

응접실 좌석은 방 남쪽에 모았다. 아이들의 정원문 진입 경로와 맥크리디의 현관 쪽 접근 경로는 가구를 피해 우회하지 않고 원래 직선 구간을 통과한다. 식탁 북쪽과 양옆에 통로를 두며, 도서실에서는 입구에서 독서 테이블·창가·북쪽 책장·수집품 갤러리에 접근할 수 있다. [가구 점유 범위와 연결 도해](../Validation/ProfessorHousePublicFinish/PublicRoomsPlan.svg).

의자 등받이는 윗부분을 별도 구체와 겹치지 않는 단일 아치형 메시로 만들었다. 천장등 고리도 닫힌 곡면으로 통합했고, 벽난로 상부는 천장까지 연결했다. 북쪽 모서리에 가까운 도서실 창은 옆 벽을 침범하는 넓은 커튼 대신 납선 테두리만 적용했다. 벽난로 불은 고정 발광 장식이며 실시간 불·연기 효과가 아니다.

## 원본과 리소스

- Blender 원본: `ArtSource/ProfessorHouse/PublicFinish_v1/ProfessorHouse_PublicFinish_v1.blend`.
- 생성 도구: `Tools/Art/build_professor_house_public_finish.py`, 공통 `professor_house_finish_kit.py`.
- Unity 모델: `Assets/@Project/10.Models/ProfessorHousePublicFinish`, 프리팹 `OBJ_ProfessorHouse_PublicFinish`.
- 임포트·방별 조명·접근 검사: `ProfessorHousePublicFinishBuilder`, 기존 `ProfessorHouseHallFinishBuilder.AddLayer` 사용.
- 25개 시각 메시, 294,158삼각형, 단순 BoxCollider 36개. 방 마감·가구 묶음·창 장식 단위로 합쳤다. 삼각형 수는 검토 단계의 수치이며 제품 성능 예산 확정값이 아니다.
- 고정 환경은 씬/프리팹의 직렬화 참조를 사용한다. 기존 Hall 그림 아틀라스·공통 재질을 재사용하며 새 Addressables 그룹이나 런타임 Resources 로드는 추가하지 않는다.
- 기존 홀·2층·전시실 마감, 확정 구조 원본, 교수 경계, S2/K0 분리, Lucy 모델, 제품 Build Settings와 LDR 기본 렌더러 0을 유지한다.

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --factory-startup --threads 4 --python-exit-code 1 --python Tools/Art/build_professor_house_public_finish.py -- --project .
```

Unity **Narnia → Professor House → Connected Layout → Build Walkthrough**로 연결 씬을 재생성한다. **Open Walkthrough**에서 Lucy로 이동한다. **Capture Unity Previews**는 현재 공용 공간 11개 시점을 촬영하고 결과를 `Docs/Validation/ProfessorHousePublicFinish`에 저장한다. 앞선 마감 레이어의 소스·생성 명령은 [전시실 문서](ProfessorHouseGalleryFinish.md), [2층 문서](ProfessorHouseUpperFinish.md), [홀 문서](ProfessorHouseHallFinish.md)를 따른다.

## 다음 제작 단계

이번 네 방도 가구·기본 마감 단계이며 세부 직물·목재 재질·생활 소품·창밖 풍경·최종 조명이 남아 있다. 다음은 1층 음악·휴게실, 수집품 갤러리와 주방·관리 구역이다. 이후 남은 2층 손님방·부속 공간, 다락·옷장방, 외관·정원을 이어 간다. 문과 사고 상호작용, 옷장방 씬 경계, 조명 베이크·구역 표시·성능 최적화와 제품 씬 통합도 후속 범위다. Lucy 모델·리깅 보완은 환경 구조와 동선이 안정된 뒤 진행한다.
