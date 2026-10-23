# 교수의 집 · 중앙 홀 마감 01

2026-10-23. [승인된 홀 시안](ProfessorHouse_FinishedConcepts_v1/Images/02_Stair_Hall_Finished.png)의 짙은 월넛, 크림색 벽, 밝은 석재, 붉은 러너와 작은 가구를 `Professor_House_Connected`에 구현한다. 확정 도면의 계단 높이·폭·계단참·회랑·출입구는 유지한다. [실제 Unity 화면](../Validation/ProfessorHouseHallFinish/index.html) · [검증 기록](../Validation/ProfessorHouseHallFinish.md).

## 구현과 소유권

- 중앙 홀의 목재·석재·러그는 전용 머티리얼로 분리한다. 홀 FBX의 노멀을 각도 기준으로 다시 계산해 난간과 구형 장식의 음영을 부드럽게 만든다. 원본 구조 FBX·승인 모형은 변경하지 않는다.
- 초상화·풍경화, 콘솔 2개, 안락의자, 흉상, 화병, 촛대, 천장 패널, 러그 문양, 창밖 나무를 별도 `OBJ_ProfessorHouse_HallFinish` 프리팹으로 구성한다. 7개 시각 메시와 가구·나무용 BoxCollider 9개다. 가구는 씬 NavMesh를 다시 구울 때 장애물로 포함된다.
- 석고 벽의 얇은 마감면이 기존 모형의 조각 경계 틈을 덮는다. 문과 창의 개구부는 승인 원본 데이터에서 가져온다.
- 그림은 built-in imagegen으로 만든 단일 텍스처 아틀라스다. 픽셀을 편집하거나 그림별 파일로 잘라내지 않고 메시 UV로 네 구역을 사용한다. [프롬프트](../../ArtSource/ProfessorHouse/HallFinish_v1/PaintingPrompt.txt)와 [생성·입력 기록](../../ArtSource/ProfessorHouse/HallFinish_v1/Provenance.json)을 보존한다.
- 고정 환경은 씬/프리팹의 직렬화 참조로 소유한다. 추가 Addressables 그룹이나 Resources 로드는 만들지 않는다. Lucy와 UI의 기존 Addressables 소유권은 유지한다.

## 조명과 렌더러

아치창 방향의 주광, 홀의 확산광, 촛대 주변 보조광을 구성했다. 연결 씬 카메라만 `ProfessorHouseHallRenderer`를 선택하며 SSAO와 홀 내부의 색 보정을 사용한다. 기존 `NarniaRenderer`와 URP 기본 렌더러 인덱스 0, LDR·그림자·추가 광원 한도는 유지한다. HDR 톤 매핑과 Bloom은 사용하지 않는다. URP에는 새 렌더러 참조 하나만 추가한다.

빛은 실시간이며 이번 단계에서 라이트맵을 베이크하지 않았다. 일반 방의 가구·천장, 정원 전체, 제품 씬 전환·동적 문·최종 성능 최적화는 후속 범위다. 중앙 홀 마감의 검토용 구현과 출시용 아트 검증을 구분한다.

## 파일과 재생성

| 내용 | 위치 |
| --- | --- |
| 가구·장식 제작 원본 | `ArtSource/ProfessorHouse/HallFinish_v1/ProfessorHouse_HallFinish_v1.blend` |
| 모델 생성 도구 | `Tools/Art/build_professor_house_hall_finish.py` |
| Unity 모델·마감 데이터 | `Assets/@Project/10.Models/ProfessorHouseHallFinish` |
| 그림 텍스처 | `Assets/@Project/03.Sprites/ProfessorHouseHallFinish/TEX_Hall_Paintings.png` |
| 홀 전용 재질 | `Assets/@Project/04.Materials/ProfessorHouseHallFinish` |
| 구성 코드 | `Assets/@Project/Editor/ProfessorHouseHallFinishBuilder.cs` |

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --factory-startup --threads 4 --python Tools/Art/build_professor_house_hall_finish.py -- --project .
```

이후 Unity 메뉴 **Narnia → Professor House → Connected Layout → Build Finished Hall**을 실행한다. 전체 연결 씬을 재생성하므로 수동 수정은 생성 원본과 코드에 먼저 반영한다. **Open Walkthrough**에서 Play하면 기존 입력으로 이동할 수 있다. **Capture Unity Previews**는 현재 씬을 그래픽 모드에서 촬영한다.

마감 추가분은 69,574삼각형, 그림 텍스처 1장이다. 추가 FBX는 Read/Write를 끄고 충돌에는 단순 박스를 사용한다. 기존 전체 연결 모형의 복잡한 구조 충돌체 정리와 구역별 표시·메모리·조명 예산 검증은 별도로 남아 있다.

다음은 2층 교수 구역·아이들 침실 등 구역별 벽·천장·가구를 제작하고, 옷장방의 씬 경계와 문·유물 사고 상호작용을 연결하는 단계다. Lucy 모델·리깅 정리는 그 뒤에 진행한다.
