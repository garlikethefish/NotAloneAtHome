extends Node
class_name ICariable

signal on_pick_up(_carrier: ICarrier)
signal on_drop(_carrier: ICarrier)

var carrier: ICarrier

func pick_up(_carrier: ICarrier):
	carrier = _carrier
	on_pick_up.emit(carrier)
	
func drop():
	on_drop.emit(carrier)
	carrier = null

func can_be_carried(_carrier: ICarrier) -> bool:
	var parent := get_parent()
	return parent != null and parent.has_method("can_be_carried") and parent.can_be_carried(_carrier)
	
