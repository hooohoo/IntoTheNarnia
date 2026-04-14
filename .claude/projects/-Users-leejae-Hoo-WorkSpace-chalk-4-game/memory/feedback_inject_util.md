---
name: FindComponents + InjectUtil 사용 필수
description: 컴포넌트 바인딩 시 transform.Find/GetComponent 대신 [FindComponents] 어트리뷰트 + InjectUtil.InjectComponents 패턴 사용
type: feedback
---

컴포넌트 바인딩은 반드시 [FindComponents("이름"), SerializeField] 어트리뷰트 + InjectUtil.InjectComponents(this) 패턴을 사용할 것. transform.Find().GetComponent<>() 직접 호출 금지.

**Why:** 프로젝트 전체가 이 패턴을 사용하고 있으며, 일관성 유지가 중요. MainTab 등 모든 UI 클래스에서 동일한 패턴 적용.

**How to apply:** MonoBehaviour든 UI_Base 상속이든, 자식 오브젝트 컴포넌트를 참조할 때는 항상 [FindComponents] + InjectUtil.InjectComponents 사용.
