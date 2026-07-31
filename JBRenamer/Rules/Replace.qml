import QtQuick
import QtQuick.Controls.Fusion
import QtQuick.Layouts
import QtQuick.Dialogs

RuleConfig {
    GridLayout {
        columns: 2
        rowSpacing: 5
        columnSpacing: 5

        Label {
            text: "Find:"
        }
        TextField {
            id: replaceConfigFindText
            Layout.fillWidth: true
        }
        Label {
            text: "Replace with:"
        }
        TextField {
            id: replaceConfigReplaceText
            Layout.fillWidth: true
        }
    }
    
    onSaveRule: function() {
        debug.log("saveRule() replace");
        rules.addReplaceRule(replaceConfigFindText.text, replaceConfigReplaceText.text);
    }
}
