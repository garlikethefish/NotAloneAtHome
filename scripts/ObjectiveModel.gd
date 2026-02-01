extends Node
class_name ObjectiveModel

enum ObjectiveName {
	HideThief,
	TakeThiefsMask,
	FeedKitty,
	CleanHome,
	WriteCode,
	Escape,
	Finish,
}

var nextObjective: ObjectiveName
var isCompleted: bool
var text: String

func _init(
	isCompleted: bool, 
	text: String, 
	nextObjective: ObjectiveName
):
	self.isCompleted = isCompleted
	self.text = text
	self.nextObjective = nextObjective
	
