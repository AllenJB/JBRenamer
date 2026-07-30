import QtQuick
import QtQuick.Controls
import QtQuick.Layouts
import QtQuick.Dialogs

RuleConfig {
    GridLayout {
        columns: 2
        rowSpacing: 5
        columnSpacing: 5

        Label {
            text: "Find Pattern:"
        }
        TextField {
            id: regexpConfigFindText
            Layout.fillWidth: true
        }
        Label {
            text: "Replace with:"
        }
        TextField {
            id: regexpConfigReplaceText
            Layout.fillWidth: true
        }
        Text {
            Layout.columnSpan: 2
            textFormat: Text.RichText
            text: "See the <a href='https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference'>Regular Expression Language Quick Reference</a>."
            onLinkActivated: (link) => program.openLink(link)
        }
    }

    onSaveRule: function() {
        debug.log("saveRule() regexp");
        rules.addRegExpRule(regexpConfigFindText.text, regexpConfigReplaceText.text);
    }
}
