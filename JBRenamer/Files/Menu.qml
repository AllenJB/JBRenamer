import QtQuick
import QtQuick.Controls.Fusion
import QtQuick.Layouts
import QtQuick.Dialogs

Menu {
    title: "&Files"

    Action {
        text: "Configure file filters"
        onTriggered: {
            filtersDialog.open()
        }
    }
    Action {
        text: "Add Source File(s)"
        onTriggered: {
            addSourceFileDialog.open()
        }
    }
    Action {
        text: "Add Source Files From Directory"
        onTriggered: {
            addSourceDirDialog.open()
        }
    }
    Action {
        text: "Rename files"
        onTriggered: {
            program.files.renameFiles(program.rules);
            program.fileListUpdated();
        }
    }
    Action {
        text: "Clear File List"
        onTriggered: {
            program.files.clear();
            program.fileListUpdated();
        }
    }
}
