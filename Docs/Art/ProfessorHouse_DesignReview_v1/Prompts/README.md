# 생성 프롬프트

모드: 내장 `image_gen`. API 키, CLI 또는 외부 이미지 모델을 사용하지 않았다.

7개 시안을 각각 생성했다. 전체 입체도는 다락 내부를 옷장 방 시안에 맞추는 수정 호출을 추가했다. 각 txt 파일에 사용한 원문 프롬프트를 보관한다. `house_master_prompt.txt` + `house_master_revision_prompt.txt`가 전체 입체도의 최종 프롬프트 묶음이다.

입력 이미지는 역할을 명시해 사용했다. 승인된 v2는 계속 유지할 형태·재질의 기준, 원본 씬은 계단 분기와 탐험 동선의 기준, 영화 스틸은 화면에 보이는 공간 요소의 근거, 층별 PNG는 제안 배치의 기준이다. 최종 시안의 경로는 `../Boards/`다. 세부 입력 대응은 프롬프트의 Input/Reference 설명과 `../Research/sources.json`을 참조한다.

평면도는 생성 모델에 의존하지 않고 직접 작성한 SVG이며 방 배치와 높이를 `spatial-contract.json`에도 함께 기록했다. 재생성용 코드는 `../Plans/make_plans.py`다.
