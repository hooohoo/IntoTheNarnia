# 아트 레퍼런스

## 홀 교차 수정·2층 마감 01 · 2026-10-23

액자와 계단·난간의 교차를 수정하고, 서재·개인 서고·교수 침실·두 아이들 침실에 가구·벽·천장·커튼·조명을 구현했다. [제작과 남은 범위](ProfessorHouseUpperFinish.md) · [최신 Unity 화면](../Validation/ProfessorHouseUpperFinish/index.html) · [검증](../Validation/ProfessorHouseUpperFinish.md). 저택 전체의 아트 배치와 조명 베이크는 진행 중이다.


## 중앙 홀 마감 01 · 2026-10-23

승인 시안에 맞춰 연결 씬 중앙 홀의 재질·난간 음영·가구·벽면 그림·천장·창빛을 구현했다. [제작과 재생성 방법](ProfessorHouseHallFinish.md) · [실제 Unity 화면](../Validation/ProfessorHouseHallFinish/index.html). 확정 계단·회랑 치수는 유지한다. 다른 구역의 마감과 제품 씬 통합은 후속 작업이다.

## 승인된 완성 분위기·연결 모형 · 2026-10-23

사용자가 새 시안 7장의 프로젝트 반영과 후속 구현을 승인했다. [완성 분위기 이미지](ProfessorHouse_FinishedConcepts_v1/index.html)는 전체 저택·계단 홀·교수 서재·여자아이 침실·유물 전시실·후정·옷장방의 재질·가구·조명 기준이다. [승인 기록](ProfessorHouse_FinishedConcepts_v1/ProjectApproval.json)과 생성·보정 프롬프트를 함께 보존한다.

[확인한 전체 연결 모형](../../ArtSource/ProfessorHouse/ConnectedLayout_v1/index.html)의 Blender/GLB·도면 데이터·재생성 도구는 `ArtSource/ProfessorHouse/ConnectedLayout_v1`에 보관한다. 확정 도면의 치수와 연결 관계를 우선하며, 컨셉 이미지의 세부 형태를 새로운 평면도로 해석하지 않는다.

[Unity 전체 이동 검증 씬](ProfessorHouseConnected.md)을 구현했다. 공통 원본에서 구역별 FBX·충돌·NavMesh를 파생했으며 실제 입력·카메라·씬 수명주기 검증을 통과했다. [Unity 화면과 결과](../Validation/ProfessorHouseConnected/index.html)를 함께 확인한다. 중앙 홀 마감은 위 01 구현으로 이어졌다. 다른 구역의 마감, 문 상호작용, 씬 전환 경계와 새 Lucy 모델은 후속 단계다.

## 확정 공간 배치 · 2026-10-22

사용자가 **층별 도면 03**을 최종 승인했다. 1층·2층·다락의 방 배치, 계단·정원 관계와 첫 안내·숨바꼭질·유물 사고 동선은 [확정 도면 v1](ProfessorHouse_ApprovedLayout_v1/index.html)을 따른다.

- [확정 기준과 적용 범위](ProfessorHouse_ApprovedLayout_v1/README.md) · [6쪽 PDF](ProfessorHouse_ApprovedLayout_v1/ProfessorHouse_FloorPlans_03.pdf) · [승인 기록](ProfessorHouse_ApprovedLayout_v1/ProjectApproval.json)
- 교수 전용 공간은 2층 서쪽, 아이들 침실은 2층 동쪽 끝, 옷장방은 다락 서쪽 끝이다. 창문·갑옷 사고는 서쪽 정원과 1층 유물 전시실에서 시작한다.
- 이전 보드의 조형·재질 참고는 유지하며, 이전 평면도·층고·방 배치는 이번 확정안을 우선한다. 전체 새 배치는 연결 모형과 Unity 이동 검증 씬에 반영했다. 제품 씬 통합과 최종 마감은 후속 제작 단계다.

## 공통 형태·캐릭터 기준: Lucy · Professor's House v2

2026-10-21 사용자 확인을 마친 모델링 참고 시안이다. Lucy, 교수의 집 메인 홀, 옷장방에 공통으로 적용할 둥글고 귀여운 로우폴리 스타일을 정의한다.

[v2 원본 이미지](References/Lucy_ProfessorHouse_Reference_v2.png)

![Lucy와 교수의 집, 옷장방 v2 레퍼런스](References/Lucy_ProfessorHouse_Reference_v2.png)

## 승인된 저택 설계 참고자료 · 2026-10-22

사용자가 보드·평면도·레퍼런스 묶음의 프로젝트 반영을 승인했다. 승인된 v2의 핵심 공간과 원본 저택의 분기 계단·여러 계단참·긴 탐험 동선을 연결하는 제작 참고자료다.

- [전체 시안과 구역별 상세](ProfessorHouse_DesignReview_v1/index.html) · [15쪽 PDF](ProfessorHouse_DesignReview_v1/ProfessorHouse_DesignReview_v1.pdf)
- 보드 7장, 층별 도면 3장, 선별 공개 레퍼런스 24장과 출처·프롬프트를 포함한다.
- [설계 기준과 판단 범위](ProfessorHouse_DesignReview_v1/README.md)

기존 자료의 공간 배치·층고는 위 확정 도면으로 대체한다. 벽 두께·계단·회랑의 세부 접속부는 확정 배치를 유지하면서 모델 제작 과정에서 검증한다. 이 자료의 프로젝트 반영과 새 구조 모델의 Unity 씬 적용은 별도 단계다. 후속 제작 검토본은 다운로드 폴더에서 먼저 확인한다.

## 자료 반영 순서

검토 중인 시안·도면·레퍼런스 묶음은 다운로드 폴더에서 확인한다. 사용자가 최종 결정한 자료만 프로젝트에 반영하고 관련 문서를 갱신한다.

## 유지할 특징

- **Lucy**: 갈색 단발과 은색 머리핀, 올리브색 카디건, 크림색 칼라·양말, 갈색 주름치마와 메리제인 구두. 둥근 얼굴과 단순한 머리 덩어리, 짧은 팔다리를 유지한다.
- **캐릭터와 공간의 비율**: 메인 홀의 비율을 기준으로 삼는다. 옷장방에서는 v1보다 Lucy를 키워 집에 비해 지나치게 작게 보이지 않도록 조정한 v2를 따른다. 시안의 화면상 크기는 실제 모델의 미터 단위 치수를 확정한 값이 아니다.
- **교수의 집**: 짙은 월넛 계열 목재, 크림색 벽, 밝은 석재 바닥, 두꺼운 난간과 구형 장식, 단순한 초상화·흉상으로 오래된 영국 저택의 인상을 유지한다. 목재를 어둡게 하되 창빛과 공간의 가독성은 유지한다.
- **옷장방**: 노출된 지붕 보, 넓은 빈 바닥, 창빛, 단독으로 놓인 옷장에 집중한다. 옷장에는 주변 목재보다 약간 선명한 창빛 하이라이트가 생기는 은은한 새틴 광택을 준다.
- **메시 구성**: 방·벽면·계단 단위의 큰 형태로 재구성한다. 문과 옷장 문처럼 움직이는 부분은 독립적으로 둔다.

## 근거와 적용 상태

- 캐릭터의 원형은 루시 리소스 추가 커밋 `e853ad46` / `bba5e51f` 및 후속 이미지 추가 커밋 `69a38d0f` / `49fb232e`다. [기존 Lucy 레퍼런스](../../Gemini_Generated_Image_uxgb2uxgb2uxgb2u.png)의 외형과 의상을 기준으로 3D 스타일을 정리했다.
- 공간은 2005년 영화 《나니아 연대기: 사자, 마녀 그리고 옷장》의 교수 Kirke의 집을 모티브로 한다. [Roger Ford 제작 디자인 인터뷰](https://www.narniaweb.com/2005/10/lecran-fantastique-interviews-roger-ford/)와 [영화 스틸](https://theadaptationstation.com/2024/01/the-lion-the-witch-and-the-wardrobe-2005-part-2-an-awfully-big-wardrobe/)을 참고했다. 이 시안은 게임 스타일로 재해석한 자료이며 영화의 정확한 평면도는 아니다.
- v2는 `imagegen`으로 제작한 참고 이미지다. 교수의 집 메인 홀·계단·2층 회랑은 이 기준으로 새 FBX·머티리얼·프리팹을 제작해 `Professor_House` 씬에 적용했다. [구현과 재생성 방법](ProfessorHouseV2.md)을 참고한다. Lucy 모델·리깅과 별도 옷장방은 후속 작업이며 기존 콘텐츠를 유지한다.
- 이 폴더는 제작 문서에 속한다. 이미지는 기존 PNG Git LFS 규칙을 따르며 Unity 런타임 에셋이나 Addressables에 등록하지 않는다.
