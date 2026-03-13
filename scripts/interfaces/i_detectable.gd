extends Helper

class_name IDetectable

signal on_detector_detecting_me(area_detector: IProximityAreaDetector)
signal on_detector_losing_me(area_detector: IProximityAreaDetector)
signal on_becoming_priority_of_detector(area_detector: IProximityAreaDetector)
signal on_not_beeing_a_priority_of_detector(area_detector: IProximityAreaDetector)

var can_be_detected_callable:   Callable
var detectors_im_inside_of:    Array[IProximityAreaDetector] = []
var detectors_im_priority_of: Array[IProximityAreaDetector] = []


func enter_area(area_detector: IProximityAreaDetector):
	if !detectors_im_inside_of.has(area_detector):
		detectors_im_inside_of.append(area_detector)
		on_detector_detecting_me.emit(area_detector)


func exit_area(area_detector: IProximityAreaDetector):
	if detectors_im_inside_of.has(area_detector):
		detectors_im_inside_of.erase(area_detector)
		on_detector_losing_me.emit(area_detector)


func set_as_area_prioriy(area_detector: IProximityAreaDetector):
	if !detectors_im_priority_of.has(area_detector):
		detectors_im_priority_of.append(area_detector)
		on_becoming_priority_of_detector.emit(area_detector)


func remove_as_area_prioriy(area_detector: IProximityAreaDetector):
	if detectors_im_priority_of.has(area_detector):
		detectors_im_inside_of.erase(area_detector)
		on_not_beeing_a_priority_of_detector.emit(area_detector)


func can_be_detected(detector: IProximityAreaDetector) -> bool:
	return can_be_detected_callable.call(detector)


func assert_assigned_callables() -> void:
	assert_callable(can_be_detected_callable, "can_be_detected_callable")
