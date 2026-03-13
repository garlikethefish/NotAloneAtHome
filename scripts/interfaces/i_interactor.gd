extends Helper

class_name IInteractor

signal on_interaction(_interactable: IInteractible)

var can_interact_callable: Callable

func interact(_interactable: IInteractible):
	if (
		_interactable == null
		or !can_interact(_interactable)
	): return
	
	_interactable.interact(self)
	on_interaction.emit(_interactable)


func can_interact(_interactable: IInteractible) -> bool:
	return can_interact_callable.call(_interactable)


func assert_assigned_callables():
	assert_callable(can_interact_callable, "can_interact_callable")
