# 교수의 집 · 전체 연결 구조의 Unity 이동 검증

2026-10-23 승인된 [완성 분위기 시안](ProfessorHouse_FinishedConcepts_v1/index.html)과 [연결 모형](../../ArtSource/ProfessorHouse/ConnectedLayout_v1/index.html)을 프로젝트에 등록했다. 확정된 50개 공간·54개 출입구와 세 장면의 동선을 실제 이동으로 확인하는 단계다.

## 이번 구현

- 원본은 `ArtSource/ProfessorHouse/ConnectedLayout_v1`, 변환 도구는 `Tools/Art/export_professor_house_connected.py`에 둔다. 원본 `.blend`를 덮어쓰지 않고 Unity용 자산을 파생한다.
- 홀·1층 방·2층 방·다락·보조계단·정원의 여섯 FBX와 환경 프리팹으로 분리한다. 각 내부 메시도 공간·유리·충돌 역할로 나뉜다.
- 계단 시각 메시와 이동용 경사 충돌체를 분리한다. 루시 1.20 m·반경 0.22 m는 공간 검증용 가정이며 최종 캐릭터 체형은 아니다.
- `Professor_House_Connected.unity`는 직접 실행하는 저작용 씬이다. 기존 매니저·입력·UI·Addressables 소유권을 사용한다. 카메라는 벽·난간을 피하도록 구성한다.
- 현재 제품 씬·태그·NavMesh Area·Build Settings를 바꾸지 않는다. 교수 구역 경계는 닫혀 있으며 나머지 문은 이동 관계를 검사하는 열린 출입구다.

## 실행과 재생성

Unity 메뉴 **Narnia → Professor House → Connected Layout → Open Walkthrough**에서 열어 Play를 누른다. 기존 WASD·방향키 또는 조이스틱으로 이동하고 기존 카메라 입력으로 시점을 조정한다.

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --factory-startup --threads 4 --python Tools/Art/export_professor_house_connected.py -- --project .
```

변환 후 Unity의 **Build Walkthrough**를 실행한다. CLI 진입점은 `ProfessorHouseConnectedBuilder.BuildBatch`다. 재생성은 연결 구조 전용 FBX·머티리얼·프리팹·씬을 다시 구성한다. 수동 변경은 원본과 생성 도구에 먼저 반영한다.

## 검증 상태

2026-10-23 Unity 2022.3.62f3에서 컴파일과 검증을 마쳤다. [상세 결과](../Validation/ProfessorHouseConnected.md)와 [실제 Unity 화면 4장](../Validation/ProfessorHouseConnected/index.html)을 함께 보관한다.

- 임포트 메시 116개 좌표·스케일 대조 통과. 시각 메시 57개, 충돌 메시 59개, 구역 프리팹 6개.
- 순서가 있는 경로 구간 182개, 캐릭터 캡슐 표본 1,599곳, 열린 출입구 53곳과 닫힌 교수 구역 경계 1곳 검사 통과.
- 후정·동측 중정의 상층 바닥 없음, 보조 계단과 침실 복도 사이의 직접 지름길 없음.
- 첫 안내·숨바꼭질·유물 사고·홀 오른쪽 계단의 186개 웨이포인트를 실제 입력으로 이동. 각 경로 시작에서만 배치하며 이동 중 순간이동하지 않는다.
- EditMode 18개(프로젝트 17 + Addressables 패키지 예제 1), PlayMode 8개 통과. 후자는 새 이동·재진입 검사 2개와 기존 게임 회귀 검사 6개다.

시각 메시 약 32.5만 삼각형·충돌 메시 약 29만 삼각형은 전체 구조 검증용 파생 자산이다. 현재 FBX는 에디터 충돌·NavMesh 검증을 위해 Read/Write를 켰으며 새 라이트맵 UV와 최종 조명 베이크를 생성하지 않았다. 실제 제품 구역에 통합하기 전에 단순 충돌체·가시 범위·메시 메모리·조명 예산을 정리한다. 이 검증은 최종 아트·모바일 성능·출시용 빌드 검증을 대신하지 않는다.

## 다음 제작 단계

[중앙 홀 마감 01](ProfessorHouseHallFinish.md)을 연결 씬에 구현했다. 이 문서의 원래 검증 기록은 마감 이전 기준이며, 최신 화면과 결과는 [중앙 홀 검증](../Validation/ProfessorHouseHallFinish.md)을 따른다. 이어 구역별 벽·천장·가구, 옷장방 독립 씬 경계, 문과 유물 사고 상호작용을 연결하고, 마지막으로 Lucy 모델·리깅을 다듬는다. 현재 원본의 일반 방·지붕·가구는 구조 확인용 형태이며 새 컨셉의 마감이 모두 구현된 상태가 아니다.
