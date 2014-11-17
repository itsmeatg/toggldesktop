// Copyright 2014 Toggl Desktop developers.

#include "./client.h"

#include <sstream>
#include <cstring>

#include "./formatter.h"

namespace toggl {

std::string Client::String() const {
    std::stringstream ss;
    ss  << "ID=" << ID()
        << " local_id=" << LocalID()
        << " name=" << name_
        << " wid=" << wid_
        << " guid=" << GUID();
    return ss.str();
}

void Client::SetName(const std::string value) {
    if (name_ != value) {
        name_ = value;
        dirty_ = true;
    }
}

void Client::SetWID(const Poco::UInt64 value) {
    if (wid_ != value) {
        wid_ = value;
        dirty_ = true;
    }
}

void Client::LoadFromJSON(Json::Value data) {
    std::string guid = data["guid"].asString();
    if (!guid.empty()) {
        SetGUID(guid);
    }

    SetID(data["id"].asUInt64());
    SetName(data["name"].asString());
    SetWID(data["wid"].asUInt64());
}

Json::Value Client::SaveToJSON() const {
    Json::Value n;
    if (ID()) {
        n["id"] = Json::UInt64(ID());
    }
    n["name"] = Formatter::EscapeJSONString(Name());
    n["wid"] = Json::UInt64(WID());
    n["guid"] = GUID();
    n["ui_modified_at"] = Json::UInt64(UIModifiedAt());
    return n;
}

}   // namespace toggl
