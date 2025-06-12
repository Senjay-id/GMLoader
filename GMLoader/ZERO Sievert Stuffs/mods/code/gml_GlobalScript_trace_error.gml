//This is for Zero Sievert
function trace_error() //gml_Script_trace_error
{
    var _string = ""
    var _i = 0
    repeat argument_count
    {
        _string += string(argument[_i])
        _i++
    }
    trace(_string)
    show_error(("Running in a modded environment, don't report the crash to the official developer.\n\nPlease attach the GMLoader.log located at the game folder when reporting the issue.\n\n\n" + _string + "\n\n "), true)
}

