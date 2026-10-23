# 중앙 홀 마감 01 제작 원본

승인된 전체 연결 모형에 덧붙이는 가구·장식 원본이다. 확정 계단·회랑·출입구를 다시 설계하지 않는다.

- `ProfessorHouse_HallFinish_v1.blend`: 7개 시각 메시. 그림 텍스처를 packed image로 포함한다.
- `PaintingPrompt.txt`: built-in imagegen에 사용한 원문 프롬프트.
- `Provenance.json`: 생성 방식, 프로젝트 텍스처 경로와 입력 해시.
- 재생성 도구: `Tools/Art/build_professor_house_hall_finish.py`.

제작 좌표는 X 동쪽, Y 후정, Z 위쪽이며 단위는 미터다. Unity용 좌표 변환은 생성 도구가 수행한다. 모델 외형 변경은 생성 도구에 먼저 반영한다. 상세 구현·재생성 방법은 [중앙 홀 문서](../../../Docs/Art/ProfessorHouseHallFinish.md)를 참고한다.
