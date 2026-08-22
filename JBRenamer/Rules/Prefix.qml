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
            text: "Prefix:"
        }
        TextField {
            id: prefixConfigValueText
            Layout.fillWidth: true
        }
        Label {
            text: "Notes:"
        }
        Label {
            text: "Use ':File_FolderName:' to insert the parent directory name"
            Layout.fillWidth: true
        }
    }

    onSaveRule: function() {
        debug.log("saveRule() prefix");
        rules.addPrefixRule(prefixConfigValueText.text);
    }
}
