# Gallery Finish v1

확정 저택의 D1 유물 전시실과 1층 F0/F1/F2, 2층 E3/F0/F1/F2/K0 마감 원본이다. 미터 단위 Blender Z-up이며 Unity (X, Z, Y)로 내보낸다.

`Tools/Art/build_professor_house_gallery_finish.py`와 공통 `professor_house_finish_kit.py`로 재생성한다. 문·창·층고는 확정 layout/model-spec을 따른다. 홀 경계는 원본 아치·문틀의 반대쪽 면을 마감한다. 그림은 기존 Hall 아틀라스를 재사용한다.

갑옷은 받침·몸통·어깨·투구 메시를 분리한다. 현재 고정 환경이며 사고 상호작용은 후속이다. 원본 ArtifactArmor/ArtifactCases는 보존하고 Unity 파생 GroundRooms에서만 제외한다.

[구성·예산·생성 명령](../../../Docs/Art/ProfessorHouseGalleryFinish.md) · [검증·실제 화면](../../../Docs/Validation/ProfessorHouseGalleryFinish.md).
