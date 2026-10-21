# Professor_House 씬 표면 정리

작업 범위: 러그의 tone_down_plane 정리 및 벽·웨인스코팅의 겹친 접합면 정리.

- tone_down_plane 34개를 제거했습니다. 기존에 실제 톤다운이 적용된 러그 8개에는 씬 전용 재질 2개를 연결했습니다.
- 기존 검은 면의 알파 0.2901961에 맞춰 재질의 밝기 계수를 0.7098039로 설정했습니다. 원본 러그 텍스처와 UV를 유지했습니다.
- 톤 보정을 지원하는 내장 Legacy Shaders/Bumped Diffuse를 사용했습니다. 신규 C# 또는 셰이더 코드는 추가하지 않았습니다.
- 벽/웨인스코팅 메시 2,657개에 접합면 정리를 적용했습니다. 완전히 중복되는 WallSet (28)/house-wall (8)의 Renderer 1개는 비활성화했습니다.
- 동일 방향의 면이 같은 평면(검사 허용 오차 0.1 mm)에서 겹치는 부분만 잘라냈습니다. 원본 프리팹, 원본 메시, 벽 재질 및 콜라이더는 유지했습니다.
- 정리한 메시 817개는 Assets/Scenes/Professor_House/Meshes/ProfessorHouse_JoinedSurfaces.asset 한 파일에 공유 메시로 보관했습니다.
- 남아 있는 오브젝트의 위치·회전·스케일·활성 상태 변경은 0개입니다. 러그 덧댐 면 외의 GameObject는 제거하지 않았습니다.
- 기존 라이트맵 37장과 Renderer의 lightmapIndex를 유지했습니다. 이번에는 라이팅을 다시 베이크하지 않았습니다.
- 주요 계단/문 접근 지점 3곳의 NavMesh 경로가 모두 PathComplete입니다. 신규 재질의 셰이더 오류와 메시/UV 누락이 없습니다.
- 촛불 Particle Renderer 3개의 빈 추가 재질 슬롯은 기존 상태입니다. 이번 작업으로 추가된 누락은 없습니다.

## 검증 자료

scene-validation.json: 배치, 삭제 대상, 라이트맵 검사.
applied-geometry-validation.json: 실제 Unity에 적용된 메시의 겹침과 표면 누락 검사. 면적 허용 오차 0.000005 m² 이내의 수치 오차를 제외한 잔여 겹침/누락 영역은 0입니다.
completion-details.json: 재질과 NavMesh 등 상세 검사.
Comparisons/: 동일 카메라 위치에서 촬영한 전후 화면.

## 백업 및 이후 편집

이 폴더의 Professor_House.unity는 이번 작업 직전에 저장되어 있던 씬 원본입니다. 사용자가 먼저 수정한 내용을 포함합니다. .meta도 함께 보관했습니다.
scene-snapshot.json, wall-geometry-before.json 및 mesh-cleanup-plan.json은 원래 상태와 적용된 표면 정리 정보를 기록합니다.

접합면 메시는 현재 고정된 배치를 기준으로 잘라 맞췄습니다. 향후 패널의 위치나 크기를 변경하는 경우 해당 접합면도 다시 맞춰야 합니다. 원본 메시로 되돌리는 것은 해당 MeshFilter의 프리팹 오버라이드를 되돌리는 방식으로 가능합니다.
러그 밝기는 Assets/Scenes/Professor_House/Materials/의 ProfessorHouse_Rug1_Toned 및 ProfessorHouse_Rug2_Toned 재질 Color로 조절할 수 있습니다.
