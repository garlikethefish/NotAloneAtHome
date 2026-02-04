extends Node
class_name ObjectiveModel

enum Objective {
	HideThief,
	TakeThiefsMask,
	FeedKitty,
	CleanHome,
	WriteCode,
	Escape,
	Finish,
}

var nextObjective: Objective
var isCompleted: bool
var text: String

func _init(
	_isCompleted: bool, 
	_text: String, 
	_nextObjective: Objective
):
	self.isCompleted = _isCompleted
	self.text = _text
	self.nextObjective = _nextObjective
